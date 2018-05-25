using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using Veldrid;

namespace Neptune.Core.Engine.Resources
{
    public class Texture: IResource
    {
        private readonly ImageTexture _imageTexture;
        private readonly Veldrid.Texture _deviceTexture;

        private Vector2 _size;
        private TextureView _textureView;
        private string _hash;

        public Vector2 Size => _size;
        public Veldrid.Texture DeviceTexture => _deviceTexture;
        public string Hash => _hash;
        public TextureView TextureView => _textureView;
        
        public Texture(ImageTexture imageTexture, Veldrid.Texture deviceTexture, TextureView textureView, string hash)
        {
            _imageTexture = imageTexture;
            _deviceTexture = deviceTexture;
            _textureView = textureView;
            _hash = hash;
            
            _size = new Vector2(_imageTexture.Width, _imageTexture.Height);
        }

        public static Texture FromFile(string path, GraphicsDevice graphicsDevice, ResourceFactory resourceFactory)
        {
            var executablePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            path = Path.Combine(Path.GetDirectoryName(executablePath), path);
            Console.WriteLine($"Loading texture at {path}");
            if (File.Exists(path))
            {
                var texture = new ImageTexture(path);
                var deviceTexture = texture.CreateDeviceTexture(graphicsDevice, resourceFactory);
                var textureView = resourceFactory.CreateTextureView(deviceTexture);
                var hash = "";
                using (var sha = SHA256Managed.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var rawHash = sha.ComputeHash(stream);
                        hash = BitConverter.ToString(rawHash).Replace("-", "").ToLowerInvariant();
                    }
                }

                return new Texture(texture, deviceTexture, textureView, hash);
            }
            else
            {
                throw new Exception($"Texture {path} you are trying to load cannot be found");
            }
        }

        public void Dispose()
        {
            _imageTexture?.Dispose();
            _deviceTexture?.Dispose();
            _textureView?.Dispose();
        }
    }
}