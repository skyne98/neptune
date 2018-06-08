using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Neptune.Core.Shaders;
using SharpShaderCompiler;
using Veldrid;

namespace Neptune.Core.Engine.Resources
{
    public class Shader: IResource
    {
        private Veldrid.Shader _vertexShader;
        private Veldrid.Shader _fragmentShader;
        private long _hash;
        private long _vertexHash;
        private long _fragmentHash;
        
        private Shader(Type t, GraphicsDevice graphicsDevice)
        {    
            var shaderType = t;
            var shaderName = shaderType.Name;

            _vertexShader = LoadShader(graphicsDevice.ResourceFactory, shaderName, ShaderStages.Vertex, "VS");
            _fragmentShader =
                LoadShader(graphicsDevice.ResourceFactory, shaderName, ShaderStages.Fragment, "PS");

            _hash = _vertexHash + _fragmentHash;
        }

        public static Shader From<T>(GraphicsDevice graphicsDevice) where T : IShader
        {
            return new Shader(typeof(T), graphicsDevice);
        }

        public Veldrid.Shader[] GetShaderSet()
        {
            return new Veldrid.Shader[] {_vertexShader, _fragmentShader};
        }
        
        public void Dispose()
        {
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
        }

        public long Hash => _hash;
        
        private Veldrid.Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
        {
            string name = $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}";
            var assetBytes = ReadEmbeddedAssetBytes(name);

            if (factory.BackendType == GraphicsBackend.Vulkan)
            {
                //Create a new compiler and new options
                var c = new ShaderCompiler();
                var o = new CompileOptions();

                //Set our compile options
                o.Language = CompileOptions.InputLanguage.GLSL;
                o.Optimization = CompileOptions.OptimizationLevel.Performance;

                //Compile the specified vertex shader and give it a name
                var r = c.Compile(Encoding.UTF8.GetString(assetBytes), stage == ShaderStages.Vertex ? ShaderCompiler.Stage.Vertex : ShaderCompiler.Stage.Fragment, o, stage == ShaderStages.Vertex ? "VS" : "PS");

                //Check if we had any compilation errors
                if (r.CompileStatus != CompileResult.Status.Success)
                {
                    //Write the error out
                    System.Console.WriteLine(r.ErrorMessage);
                    throw new Exception("Cannot compile Vulkan shader");
                }

                //Get the produced SPV bytecode
                assetBytes = r.GetBytes();
            }

            var hash = 0l;
            using (var sha = SHA256Managed.Create())
            {
                using (var stream = new MemoryStream(assetBytes))
                {
                    var rawHash = sha.ComputeHash(stream);
                    hash = BitConverter.ToInt64(rawHash, 0);
                }
            }
            
            if (stage == ShaderStages.Vertex)
            {
                _vertexHash = hash;
            }
            else
            {
                if (stage == ShaderStages.Fragment)
                {
                    _fragmentHash = hash;
                }
            }
            
            return factory.CreateShader(new ShaderDescription(stage, assetBytes, entryPoint));
        }

        private byte[] ReadEmbeddedAssetBytes(string name)
        {
            using (Stream stream = OpenEmbeddedAssetStream(name))
            {
                byte[] bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }
        
        private static string GetExtension(GraphicsBackend backendType)
        {
            bool isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");

            return (backendType == GraphicsBackend.Direct3D11)
                ? "hlsl.bytes"
                : (backendType == GraphicsBackend.Vulkan)
                    ? "450.glsl"
                    : (backendType == GraphicsBackend.Metal)
                        ? isMacOS ? "metallib" : "ios.metallib"
                        : (backendType == GraphicsBackend.OpenGL)
                            ? "330.glsl"
                            : "300.glsles";
        }

        private Stream OpenEmbeddedAssetStream(string name)
        {
            var type = GetType();
            var assembly = type.Assembly;
            var m = assembly.GetManifestResourceNames();
            
            return assembly.GetManifestResourceStream(name);
        }
    }
}