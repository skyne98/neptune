using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;

namespace Neptune.Core.Engine.Resources
{
    public class ResourceManager
    {
        private GraphicsDevice _graphicsDevice;
        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        public ResourceManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void LoadTexture(string path, string name, bool fallback = true)
        {
            try
            {
                if (_textures.ContainsKey(name) == false)
                {
                    var texture = Texture2D.FromFile(path, _graphicsDevice, _graphicsDevice.ResourceFactory);
                    _textures.Add(name, texture);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {Environment.NewLine} {ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error occured: {Environment.NewLine} {ex}");
            }
        }

        public Texture2D GetTexture(string name)
        {
            if (_textures.ContainsKey(name))
            {
                var texture = _textures[name];
                return texture;
            }

            throw new Exception($"Texture {name} was not loaded");
        }
    }
}
