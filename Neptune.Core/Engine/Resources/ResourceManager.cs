using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Neptune.Core.Shaders;
using Veldrid;
using Vulkan;

namespace Neptune.Core.Engine.Resources
{
    public class ResourceManager: IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        
        private List<IDisposable> _disposables = new List<IDisposable>();
        private Dictionary<string, ResourceLink<Texture>> _textures = new Dictionary<string, ResourceLink<Texture>>();
        private Dictionary<string, ResourceLink<Shader>> _shaders = new Dictionary<string, ResourceLink<Shader>>();

        public ResourceManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public ResourceLink<Texture> LoadTexture(string path, string name)
        {
            try
            {
                var texture = Texture.FromFile(path, _graphicsDevice, _graphicsDevice.ResourceFactory);
                _disposables.Add(texture);
                if (_textures.ContainsKey(name) == false)
                {
                    _textures.Add(name, new ResourceLink<Texture>(texture));
                }
                else
                {
                    _textures[name].Set(texture);   
                }

                return _textures[name];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while trying to load {name} from {path}");
                throw;
            }
        }

        public ResourceLink<Shader> LoadShader<T>(string name) where T: IShader
        {
            try
            {
                var shaderResource = Shader.From<T>(_graphicsDevice);
                    
                _disposables.Add(shaderResource);
                if (_shaders.ContainsKey(name) == false)
                {
                    _shaders.Add(name, new ResourceLink<Shader>(shaderResource));
                }
                else
                {
                    _shaders[name].Set(shaderResource);   
                }

                return _shaders[name];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while trying to load {name}");
                throw;
            }
        }

        public ResourceLink<Texture> GetTexture(string name)
        {
            if (_textures.ContainsKey(name))
            {
                var texture = _textures[name];
                return texture;
            }

            throw new Exception($"Texture {name} is not loaded");
        }

        public ResourceLink<Shader> GetShader(string name)
        {
            if (_shaders.ContainsKey(name))
            {
                var shader = _shaders[name];
                return shader;
            }
            
            throw new Exception($"Shader {name} is not loaded");
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
