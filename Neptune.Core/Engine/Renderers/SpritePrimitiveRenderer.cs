using Neptune.Core.Engine.Primitives;
using System;
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

        private Dictionary<string, List<SpritePrimitive>> _groupSprites;
        private Dictionary<string, ResourceLink<Texture>> _groupTextures;
        private Dictionary<string, ResourceSet> _groupResourceSets;
        private Dictionary<string, bool> _groupInfoDirty;
        private Dictionary<string, bool> _groupMatricesDirty;
        private Dictionary<string, InstanceInfo[]> _groupInstanceInfos;
        private Dictionary<string, DeviceBuffer> _groupInstanceInfoBuffers;
        private Dictionary<string, DeviceBuffer> _groupMatrixBuffers;

        private List<SpritePrimitive> _dirtySprites;

        private ResourceLink<Resources.Shader> _shader;

        public SpritePrimitiveRenderer(GraphicsDevice graphicsDevice, CommandList commandsList, Sdl2Window window, ResourceManager resourceManager)
        {
            _graphicsDevice = graphicsDevice;
            _commandList = commandsList;
            _window = window;
            _resourceManager = resourceManager;
            _resourceFactory = _graphicsDevice.ResourceFactory;

            _groupSprites = new Dictionary<string, List<SpritePrimitive>>();
            _groupTextures = new Dictionary<string, ResourceLink<Texture>>();
            _groupResourceSets = new Dictionary<string, ResourceSet>();
            _groupInfoDirty = new Dictionary<string, bool>();
            _groupMatricesDirty = new Dictionary<string, bool>();
            _groupInstanceInfos = new Dictionary<string, InstanceInfo[]>();
            _groupInstanceInfoBuffers = new Dictionary<string, DeviceBuffer>();
            _groupMatrixBuffers = new Dictionary<string, DeviceBuffer>();

            _dirtySprites = new List<SpritePrimitive>();

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
                    new VertexElementDescription("ZIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                    new VertexElementDescription("MatrixIndex", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1), 
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
                new ResourceLayoutElementDescription("ModelMatrices", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex)
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
                var matrixBufferDescription = new BufferDescription(64 * 1000000,
                    BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic);
                matrixBufferDescription.StructureByteStride = 64;
                var matrixBuffer = _resourceFactory.CreateBuffer(matrixBufferDescription);
                _groupMatrixBuffers.Add(primitive.Texture.Hash, matrixBuffer);
                _groupSprites.Add(primitive.Texture.Hash, new List<SpritePrimitive>());
                _groupTextures.Add(primitive.Texture.Hash, primitive.TextureLink);
                _groupResourceSets.Add(primitive.Texture.Hash, _resourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _groupResourceLayout,
                        _groupTextures[primitive.Texture.Hash].Get().TextureView,
                        _graphicsDevice.PointSampler,
                        matrixBuffer
                    )));
                _groupInfoDirty.Add(primitive.Texture.Hash, true);
                _groupMatricesDirty.Add(primitive.Texture.Hash, true);
                _groupInstanceInfos.Add(primitive.Texture.Hash, null);
                _groupInstanceInfoBuffers.Add(primitive.Texture.Hash,
                    _resourceFactory.CreateBuffer(new BufferDescription(2000000 * InstanceInfo.SizeInBytes,
                        BufferUsage.VertexBuffer)));
            }

            _groupSprites[primitive.Texture.Hash].Add(primitive);
            _groupInfoDirty[primitive.Texture.Hash] = true;
            
            AddDirty(primitive);
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

        internal void AddDirty(SpritePrimitive primitive)
        {
            _dirtySprites.Add(primitive);
        }

        private void ProcessDirtySprites()
        {
            if (_dirtySprites.Count > 0)
            {
                // Update the matrices
                Console.WriteLine($"Processing {_dirtySprites.Count} dirty sprites");
                var transforms = _dirtySprites.Select(s => s.NativeTransform).ToArray();
                var matrices = _dirtySprites.Select(s => s.ModelMatrix).ToArray();
                ParallelNative.CalculateMatrices(transforms, matrices);

                int index = 0;
                foreach (var spritePrimitive in _dirtySprites)
                {
                    var matrix = matrices[index];
                    spritePrimitive.ModelMatrix = matrix;

                    _groupMatricesDirty[spritePrimitive.Texture.Hash] = true;
                    spritePrimitive._dirty = false;
                    index = index + 1;
                }
                _dirtySprites.Clear();
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
                            s.ZIndex,
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

                if (_groupMatricesDirty[groupSpritesKey])
                {
                    var groupMatrices = _groupSprites[groupSpritesKey].Select(s => s.ModelMatrix).ToArray();
                    _graphicsDevice.UpdateBuffer(_groupMatrixBuffers[groupSpritesKey], 0, groupMatrices);
                    _groupMatricesDirty[groupSpritesKey] = false;
                }
            }
        }

        public void Render()
        {
            // Process the dirty sprites
            ProcessDirtySprites();

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