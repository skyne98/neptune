using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Neptune.Core.Shaders;
using Veldrid;

namespace Neptune.Core.Engine.Resources
{
    public class Shader: IResource
    {
        private Veldrid.Shader _vertexShader;
        private Veldrid.Shader _fragmentShader;
        private string _hash;
        private string _vertexHash;
        private string _fragmentHash;
        
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

        public string Hash => _hash;
        
        private Veldrid.Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
        {
            string name = $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}";
            var assetBytes = ReadEmbeddedAssetBytes(name);
            
            var hash = "";
            using (var sha = SHA256Managed.Create())
            {
                using (var stream = new MemoryStream(assetBytes))
                {
                    var rawHash = sha.ComputeHash(stream);
                    hash = BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
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
                    ? "450.glsl.spv"
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