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
        private ResourceSet _groupResourceSet;
        private ResourceLayout _spriteResourceLayout;

        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projectionMatrixBuffer;
        
        private Vector2 _cameraPosition = Vector2.Zero;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        private Dictionary<string, List<SpritePrimitive>> _groupedSprites;
        private ResourceLink<Resources.Shader> _shader;

        public SpritePrimitiveRenderer(GraphicsDevice graphicsDevice, CommandList commandsList, Sdl2Window window, ResourceManager resourceManager)
        {
            _graphicsDevice = graphicsDevice;
            _commandList = commandsList;
            _window = window;
            _resourceManager = resourceManager;
            _resourceFactory = _graphicsDevice.ResourceFactory;
            _groupedSprites = new Dictionary<string, List<SpritePrimitive>>();

            InitializeResources();
        }

        public void Dispose()
        {
            _pipeline.Dispose();
            _indexBuffer.Dispose();
            _projectionMatrixBuffer.Dispose();
            _groupResourceLayout.Dispose();
            _groupResourceSet.Dispose();
            _spriteResourceLayout.Dispose();
        }

        public void Add(SpritePrimitive primitive)
        {
            if (_groupedSprites.ContainsKey(primitive.Texture.Hash) == false)
            {
                _groupedSprites.Add(primitive.Texture.Hash, new List<SpritePrimitive>());
            }

            _groupedSprites[primitive.Texture.Hash].Add(primitive);
        }

        public void Remove(SpritePrimitive primitive)
        {
            if (_groupedSprites.ContainsKey(primitive.Texture.Hash))
            {
                _groupedSprites[primitive.Texture.Hash].Remove(primitive);
            }
        }

        public void Render()
        {
            // Set the pipeline
            _commandList.SetPipeline(_pipeline);
            
            // Update the projection matrix
            _projectionMatrix = Matrix4x4.CreateOrthographic(_window.Width, _window.Height, 0.0f, 10.0f);

            // Update the buffers
            var spriteCameraMatrix = _projectionMatrix;
            _graphicsDevice.UpdateBuffer(_projectionMatrixBuffer, 0, ref spriteCameraMatrix);

            foreach (var key in _groupedSprites.Keys)
            {
                var texture = _groupedSprites[key][0].Texture;
                // Create the group resource set
                _groupResourceSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(
                        _groupResourceLayout,
                        _projectionMatrixBuffer,
                        texture.TextureView,
                        _graphicsDevice.PointSampler
                ));
                _commandList.SetGraphicsResourceSet(1, _groupResourceSet);

                var group = _groupedSprites[key];
                var groupDirty = group.Where(s => s.Dirty);

                var groupTransforms = groupDirty.Select(s => s.NativeTransform).ToArray();
                var matrices = groupDirty.Select(s => s.ModelMatrix).ToArray();
                ParallelNative.CalculateMatrices(groupTransforms, matrices);

                int index = 0;
                foreach (var spritePrimitive in groupDirty)
                {
                    var matrix = matrices[index];
                    spritePrimitive.ModelMatrix = matrix;
                    
                    // Update the vertices
                    var vertices = new List<VertexInfo>()
                    {
                        new VertexInfo(new Vector2(0, 1), new Vector2(0, 1), spritePrimitive.Color),
                        new VertexInfo(new Vector2(1, 1), new Vector2(1, 1), spritePrimitive.Color),
                        new VertexInfo(new Vector2(0, 0), new Vector2(0, 0), spritePrimitive.Color),
                        new VertexInfo(new Vector2(1, 0), new Vector2(1, 0), spritePrimitive.Color)
                    };
                    spritePrimitive.Vertices = vertices;

                    _graphicsDevice.UpdateBuffer(spritePrimitive.VertexBuffer, 0, vertices.ToArray());
                    _graphicsDevice.UpdateBuffer(spritePrimitive.ModelMatrixBuffer, 0, spritePrimitive.ModelMatrix);
                    _graphicsDevice.UpdateBuffer(spritePrimitive.ZIndexBuffer, 0, spritePrimitive.ZIndex);

                    spritePrimitive.Dirty = false;
                    index = index + 1;
                }
                
                foreach (var sprite in group)
                {
                    _commandList.SetVertexBuffer(0, sprite.VertexBuffer);
                    _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

                    // Update the resource set
                    if (sprite.ResourceSet == null)
                    {
                        sprite.ResourceSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(
                            _spriteResourceLayout,
                            sprite.ModelMatrixBuffer,
                            sprite.ZIndexBuffer
                        ));
                    }
                    _commandList.SetGraphicsResourceSet(0, sprite.ResourceSet);
                    _commandList.DrawIndexed(
                        indexCount: 4,
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                }
            }
        }

        private void InitializeResources()
        {
            ushort[] quadIndices = {0, 1, 2, 3};
            _indexBuffer =
                _resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));
            _projectionMatrixBuffer =
                _resourceFactory.CreateBuffer(
                    new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4)
            );

            _shader = _resourceManager.LoadShader<SpriteShader>("SpriteShader");

            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = RasterizerStateDescription.CullNone,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };

            _spriteResourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("ZIndex", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            _groupResourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            pipelineDescription.ResourceLayouts = new[]
            {
                _spriteResourceLayout,
                _groupResourceLayout
            };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                shaders: _shader.Get().GetShaderSet());

            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }
    }
}