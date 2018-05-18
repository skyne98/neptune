using Neptune.Core.Engine.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Vulkan;

// TODO: Optimizations
// Geometry streaming: http://voidptr.io/blog/2016/04/28/ldEngine-Part-1.html

namespace Neptune.Core.Engine.Renderers
{
    public class SpritePrimitiveRenderer : IRenderer<SpriteRenderingPrimitive>, IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        private CommandList _commandList;
        private readonly Sdl2Window _window;
        private ResourceFactory _resourceFactory;
        private Pipeline _pipeline;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _resourceLayout;
        private ResourceSet _resourceSet;

        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projectionMatrixBuffer;
        
        private Vector2 _cameraPosition = Vector2.Zero;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        public SpritePrimitiveRenderer(GraphicsDevice graphicsDevice, CommandList commandsList, Sdl2Window window)
        {
            _graphicsDevice = graphicsDevice;
            _commandList = commandsList;
            _window = window;
            _resourceFactory = _graphicsDevice.ResourceFactory;

            InitializeResources();
        }

        public void Dispose()
        {
            _pipeline.Dispose();
            _indexBuffer.Dispose();
            _projectionMatrixBuffer.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _resourceLayout.Dispose();
        }

        public void Render(List<SpriteRenderingPrimitive> primitives)
        {
            // Set the pipeline
            _commandList.SetPipeline(_pipeline);
            
            // Update the projection matrix
            _projectionMatrix = Matrix4x4.CreateOrthographic(_window.Width, _window.Height, 0.0f, 10.0f);

            foreach (var sprite in primitives)
            {
                if (sprite.Dirty)
                {
                    // FROM: https://learnopengl.com/In-Practice/2D-Game/Rendering-Sprites
                    var size = new Vector2(sprite.Size.X * sprite.Scale.X, sprite.Size.Y * sprite.Scale.Y);
                    var modelMatrix = Matrix4x4.CreateScale(new Vector3(size, 1.0f));
                    modelMatrix = modelMatrix * Matrix4x4.CreateTranslation(new Vector3(-sprite.Origin.X * size.X, -sprite.Origin.Y * size.Y, 0.0f));
                    modelMatrix = modelMatrix * Matrix4x4.CreateRotationZ(sprite.Rotation);
                    modelMatrix = modelMatrix * Matrix4x4.CreateTranslation(new Vector3(sprite.Position, 0));
                    sprite.ModelMatrix = modelMatrix;

                    // Update the vertices
                    var vertices = new List<VertexInfo>()
                    {
                        new VertexInfo(new Vector2(0, 1), new Vector2(0, 1), sprite.Color),
                        new VertexInfo(new Vector2(1, 1), new Vector2(1, 1), sprite.Color),
                        new VertexInfo(new Vector2(0, 0), new Vector2(0, 0), sprite.Color),
                        new VertexInfo(new Vector2(1, 0), new Vector2(1, 0), sprite.Color)
                    };
                    sprite.Vertices = vertices;
                    
                    _graphicsDevice.UpdateBuffer(sprite.VertexBuffer, 0, vertices.ToArray());
                    _graphicsDevice.UpdateBuffer(sprite.ModelMatrixBuffer, 0, sprite.ModelMatrix);

                    sprite.Dirty = false;
                }

                // Update the buffers
                var spriteCameraMatrix = _projectionMatrix;
                _graphicsDevice.UpdateBuffer(_projectionMatrixBuffer, 0, ref spriteCameraMatrix);
                
                _commandList.SetVertexBuffer(0, sprite.VertexBuffer);
                _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

                // Update the resource set
                _resourceSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _resourceLayout,
                    sprite.ModelMatrixBuffer,
                    _projectionMatrixBuffer,
                    sprite.Texture.TextureView
                ));
                _commandList.SetGraphicsResourceSet(0, _resourceSet);
                _commandList.DrawIndexed(
                    indexCount: 4,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);
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

            _vertexShader = LoadShader("SpriteShader", ShaderStages.Vertex);
            _fragmentShader = LoadShader("SpriteShader", ShaderStages.Fragment);

            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = RasterizerStateDescription.CullNone;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            _resourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)
            ));
            pipelineDescription.ResourceLayouts = new[]
            {
                _resourceLayout
            };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] {vertexLayout},
                shaders: new Shader[] {_vertexShader, _fragmentShader});

            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        private Shader LoadShader(string name, ShaderStages stage)
        {
            string extension = null;
            switch (_graphicsDevice.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    extension = "hlsl.bytes";
                    break;
                case GraphicsBackend.Vulkan:
                    extension = "450.glsl.spv";
                    break;
                case GraphicsBackend.OpenGL:
                    extension = "330.glsl";
                    break;
                case GraphicsBackend.OpenGLES:
                    extension = "300.glsles";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory, "Shaders.Generated",
                $"{name}-{stage.ToString().ToLower()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return _graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }
    }
}