using Neptune.Core.Engine.Primitives;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Neptune.Core.Engine.Resources;
using Neptune.Core.Shaders;
using Neptune.JobSystem.Native;
using Veldrid;
using Veldrid.Sdl2;
using Vulkan;
using Shader = Veldrid.Shader;
using Texture = Neptune.Core.Engine.Resources.Texture;

// TODO: Optimizations
// Geometry streaming: http://voidptr.io/blog/2016/04/28/ldEngine-Part-1.html

namespace Neptune.Core.Engine.Renderers
{
    public class SpritePrimitiveRenderer : IRenderer<SpritePrimitive>, IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private CommandList _commandList;
        private readonly Sdl2Window _window;
        private readonly ResourceManager _resourceManager;
        private ResourceFactory _resourceFactory;
        private Pipeline _pipeline;
        private ResourceLayout _groupResourceLayout;
        private ResourceLayout _cameraResourceLayout;
        private ResourceSet _cameraResourceSet;

        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _projectionMatrixBuffer;
        
        private Vector2 _cameraPosition = Vector2.Zero;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        private Dictionary<long, List<SpritePrimitive>> _groupSprites;
        private Dictionary<long, ResourceLink<Texture>> _groupTextures;
        private Dictionary<long, ResourceSet> _groupResourceSets;
        private Dictionary<long, bool> _groupInfoDirty;
        private Dictionary<long, bool> _groupTransformDirty;
        private Dictionary<long, InstanceInfo[]> _groupInstanceInfos;
        private Dictionary<long, DeviceBuffer> _groupInstanceInfoBuffers;
        private Dictionary<long, DeviceBuffer> _groupTransformDataBuffers;
        private Dictionary<long, SpriteShader.TransformData[]> _groupTransformDatas;

        private List<SpritePrimitive> _dirtySprites;

        private ResourceLink<Resources.Shader> _shader;

        // Unique index
        private int _lastIndex = 0;

        public SpritePrimitiveRenderer(GraphicsDevice graphicsDevice, CommandList commandsList, Sdl2Window window, ResourceManager resourceManager)
        {
            _graphicsDevice = graphicsDevice;
            _commandList = commandsList;
            _window = window;
            _resourceManager = resourceManager;
            _resourceFactory = _graphicsDevice.ResourceFactory;

            _groupSprites = new Dictionary<long, List<SpritePrimitive>>();
            _groupTextures = new Dictionary<long, ResourceLink<Texture>>();
            _groupResourceSets = new Dictionary<long, ResourceSet>();
            _groupInfoDirty = new Dictionary<long, bool>();
            _groupTransformDirty = new Dictionary<long, bool>();
            _groupInstanceInfos = new Dictionary<long, InstanceInfo[]>();
            _groupInstanceInfoBuffers = new Dictionary<long, DeviceBuffer>();
            _groupTransformDataBuffers = new Dictionary<long, DeviceBuffer>();
            _groupTransformDatas = new Dictionary<long, SpriteShader.TransformData[]>();

            InitializeResources();
        }

        private void InitializeResources()
        {
            ushort[] quadIndices = { 0, 1, 2, 3 };
            _indexBuffer =
                _resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));
            _vertexBuffer =
                _resourceFactory.CreateBuffer(new BufferDescription(4 * VertexInfo.SizeInBytes, BufferUsage.VertexBuffer));
            _projectionMatrixBuffer =
                _resourceFactory.CreateBuffer(
                    new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            // Update the index buffers
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);
            // Update the vertex buffer
            var vertices = new VertexInfo[]
            {
                new VertexInfo(new Vector2(0, 1), new Vector2(0, 1)),
                new VertexInfo(new Vector2(1, 1), new Vector2(1, 1)),
                new VertexInfo(new Vector2(0, 0), new Vector2(0, 0)),
                new VertexInfo(new Vector2(1, 0), new Vector2(1, 0))
            };
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            );
            VertexLayoutDescription instanceVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription[]
                {
                    new VertexElementDescription("TransformDataIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1), 
                    new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4) 
                }
            );
            instanceVertexLayout.InstanceStepRate = 1;

            _shader = _resourceManager.LoadShader<SpriteShader>("SpriteShader");

            var rasterizerState = RasterizerStateDescription.Default;
            rasterizerState.DepthClipEnabled = false;
            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = rasterizerState,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };

            _cameraResourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            _cameraResourceSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(
                _cameraResourceLayout,
                _projectionMatrixBuffer
            ));

            _groupResourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("TransformDatas", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex)
            ));

            pipelineDescription.ResourceLayouts = new[]
            {
                _cameraResourceLayout,
                _groupResourceLayout
            };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout, instanceVertexLayout },
                shaders: _shader.Get().GetShaderSet());

            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        public void Dispose()
        {
            _pipeline.Dispose();
            _indexBuffer.Dispose();
            _projectionMatrixBuffer.Dispose();
            _groupResourceLayout.Dispose();
            _cameraResourceLayout.Dispose();
        }

        public void Add(SpritePrimitive primitive)
        {
            if (_groupSprites.ContainsKey(primitive.Texture.Hash) == false)
            {
                // Create a new group
                var transformDataBufferDescription = new BufferDescription(32 * 1300000,
                    BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic);
                transformDataBufferDescription.StructureByteStride = 32;
                var transformDataBuffer = _resourceFactory.CreateBuffer(transformDataBufferDescription);
                _groupTransformDataBuffers.Add(primitive.Texture.Hash, transformDataBuffer);
                _groupSprites.Add(primitive.Texture.Hash, new List<SpritePrimitive>());
                _groupTextures.Add(primitive.Texture.Hash, primitive.TextureLink);
                _groupResourceSets.Add(primitive.Texture.Hash, _resourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _groupResourceLayout,
                        _groupTextures[primitive.Texture.Hash].Get().TextureView,
                        _graphicsDevice.PointSampler,
                        transformDataBuffer
                    )));
                _groupInfoDirty.Add(primitive.Texture.Hash, true);
                _groupTransformDirty.Add(primitive.Texture.Hash, true);
                _groupInstanceInfos.Add(primitive.Texture.Hash, null);
                _groupInstanceInfoBuffers.Add(primitive.Texture.Hash,
                    _resourceFactory.CreateBuffer(new BufferDescription(2000000 * InstanceInfo.SizeInBytes,
                        BufferUsage.VertexBuffer)));
                _groupTransformDatas.Add(primitive.Texture.Hash, ArrayPool<SpriteShader.TransformData>.Shared.Rent(1));
            }

            _groupSprites[primitive.Texture.Hash].Add(primitive);
            _groupInfoDirty[primitive.Texture.Hash] = true;
            
            primitive._spritePrimitiveRenderer = this;
        }

        public void Remove(SpritePrimitive primitive)
        {
            if (_groupSprites.ContainsKey(primitive.Texture.Hash))
            {
                _groupSprites[primitive.Texture.Hash].Remove(primitive);
                _groupInfoDirty[primitive.Texture.Hash] = true;
                primitive._spritePrimitiveRenderer = null;
            }
        }

        public void SetGroupDirty(long group)
        {
            if (_groupTransformDirty.ContainsKey(group))
            {
                _groupTransformDirty[group] = true;
            }
        }

        private void ProcessTransformChanges()
        {
            foreach (var groupSpritesKey in _groupSprites.Keys)
            {
                if (_groupTransformDirty[groupSpritesKey])
                {
                    // Resize the transforms array, if needed
                    if (_groupTransformDatas[groupSpritesKey].Length != _groupSprites[groupSpritesKey].Count)
                    {
                        // Return the used array
                        ArrayPool<SpriteShader.TransformData>.Shared.Return(_groupTransformDatas[groupSpritesKey]);

                        // Rent a new one
                        var newArray =
                            ArrayPool<SpriteShader.TransformData>.Shared.Rent(_groupSprites[groupSpritesKey].Count);
                        _groupTransformDatas[groupSpritesKey] = newArray;
                    }

                    var writeMap = _graphicsDevice.Map<SpriteShader.TransformData>(_groupTransformDataBuffers[groupSpritesKey],
                        MapMode.Write);
                    var spritesGroup = _groupSprites[groupSpritesKey];
                    var datas = _groupTransformDatas[groupSpritesKey];
                    for (int i = 0; i < spritesGroup.Count; i++)
                    {
                        var sprite = spritesGroup[i];

                        writeMap[i].Position = new Vector3(sprite.Position, sprite.ZIndex);
                        writeMap[i].Rotation = sprite.Rotation;
                        writeMap[i].Size = sprite.Size;
                        writeMap[i].Origin = sprite.Origin;
                    }
                    
                    _graphicsDevice.Unmap(_groupTransformDataBuffers[groupSpritesKey]);
                }
            }
        }

        private void ProcessChanges()
        {
            foreach (var groupSpritesKey in _groupSprites.Keys)
            {
                if (_groupInfoDirty[groupSpritesKey])
                {
                    // Update the instance infos
                    var index = 0;
                    var newInfos = _groupSprites[groupSpritesKey].Select(s =>
                    {
                        var info =  new InstanceInfo(
                            index,
                            s.Color
                        );

                        index = index + 1;
                        return info;
                    }).ToArray();
                    _groupInstanceInfos[groupSpritesKey] = newInfos;
                    _graphicsDevice.UpdateBuffer(_groupInstanceInfoBuffers[groupSpritesKey], 0, newInfos);
                    _groupInfoDirty[groupSpritesKey] = false;
                }
            }
        }

        public void Render()
        {
            // Process the dirty groups (transform changes)
            ProcessTransformChanges();

            // Process the dirty groups (additions and removals)
            ProcessChanges();

            // Set the pipeline
            _commandList.SetPipeline(_pipeline);

            // Update the projection matrix
            _projectionMatrix = Matrix4x4.CreateOrthographic(_window.Width, _window.Height, 0.0f, 10.0f);
            _graphicsDevice.UpdateBuffer(_projectionMatrixBuffer, 0, ref _projectionMatrix);
            _commandList.SetGraphicsResourceSet(0, _cameraResourceSet);

            // Set vertex and index buffers
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            foreach (var key in _groupSprites.Keys)
            {
                var texture = _groupTextures[key];
                var resourceSet = _groupResourceSets[key];
                _commandList.SetGraphicsResourceSet(1, resourceSet);

                // Set the instance infos
                _commandList.SetVertexBuffer(1, _groupInstanceInfoBuffers[key]);

                _commandList.DrawIndexed(
                    indexCount: 4,
                    instanceCount: (uint)_groupSprites[key].Count, 
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }
}