using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SharpDX.DXGI;
using Veldrid;

namespace Neptune.Engine.Sample
{
    public class SimpleRenderer: IRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private CommandList _commandList;
        private List<DeviceBuffer> _vertexBuffers;
        private List<DeviceBuffer> _indexBuffers;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline _pipeline;
        private ResourceSet _resourceSet;
        private ResourceFactory _resourceFactory;

        private uint _instanceCount; 

        public SimpleRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            
            CreateResources();
        }
        
        public void BatchDraw(Action<ISpriteBatch> action)
        {
            var batch = new SimpleBatch();
            action(batch);
            var (vertices, triangles) = batch.GetTriangles();
            UpdateBuffers(vertices, triangles);
            Draw();
        }
        
        private void Draw()
        {
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            for (var i = 0; i < _vertexBuffers.Count; i++)
            {
                _commandList.SetVertexBuffer(0, _vertexBuffers[i]);
                _commandList.SetIndexBuffer(_indexBuffers[i], IndexFormat.UInt16);
                _commandList.SetPipeline(_pipeline);
                _commandList.SetGraphicsResourceSet(0, _resourceSet);
                _commandList.DrawIndexed(
                    indexCount: 4,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0);   
            }
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
        }
        
        public void DisposeResources()
        {
            _pipeline.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _commandList.Dispose();
            _graphicsDevice.Dispose();
            foreach (var vertexBuffer in _vertexBuffers)
            {
                 vertexBuffer.Dispose();
            }

            foreach (var indexBuffer in _indexBuffers)
            {
                indexBuffer.Dispose();
            }
        }

        private void CreateResources()
        {
            _resourceFactory = _graphicsDevice.ResourceFactory;

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("vin_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("vin_texcoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("vin_color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            _vertexShader = LoadShader(ShaderStages.Vertex);
            _fragmentShader = LoadShader(ShaderStages.Fragment);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            var resourceLayout = _resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("sprite_texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)
            ));
            
            pipelineDescription.ResourceLayouts = new[]
            {
                resourceLayout
            };

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: new Shader[] { _vertexShader, _fragmentShader });

            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _resourceFactory.CreateCommandList();
            
            // Create the resource set
            var texture = new ImageTexture("Assets/doge.jpg").CreateDeviceTexture(_graphicsDevice, _resourceFactory);
            var view = _resourceFactory.CreateTextureView(texture);
            
            _resourceSet = _resourceFactory.CreateResourceSet(new ResourceSetDescription(
                resourceLayout,
                view
            ));   
        }

        private void UpdateBuffers(List<VertexInfo> vertices, List<ushort> triangles)
        {
            _vertexBuffers = new List<DeviceBuffer>();
            _indexBuffers = new List<DeviceBuffer>();
            
            for (var i = 0; i < triangles.Count / 4; i++)
            {
                _vertexBuffers.Add(_resourceFactory.CreateBuffer(new BufferDescription(VertexInfo.SizeInBytes * 4, BufferUsage.VertexBuffer)));
                _indexBuffers.Add(_resourceFactory.CreateBuffer(new BufferDescription(sizeof(ushort) * 4, BufferUsage.IndexBuffer)));
                
                _graphicsDevice.UpdateBuffer(_vertexBuffers[i], 0, vertices.GetRange(i * 4, 4).ToArray());
                _graphicsDevice.UpdateBuffer(_indexBuffers[i], 0, triangles.GetRange(i * 4, 4).ToArray());
            }
        }
        
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private Shader LoadShader(ShaderStages stage)
        {
            string extension = null;
            switch (_graphicsDevice.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    extension = "hlsl.bytes";
                    break;
                case GraphicsBackend.Vulkan:
                    extension = "spv";
                    break;
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory, "Shaders", $"{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return _graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }    
    }
}