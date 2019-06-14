using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using Veldrid;
using Veldrid.ImageSharp;

namespace Neptune.Core.Engine.Resources
{
    public class Texture : IResource
    {
        private readonly ImageSharpTexture _imageTexture;
        private readonly Veldrid.Texture _deviceTexture;

        private Vector2 _size;
        private TextureView _textureView;
        private long _hash;

        public Vector2 Size => _size;
        public Veldrid.Texture DeviceTexture => _deviceTexture;
        public long Hash => _hash;
        public TextureView TextureView => _textureView;

        public Texture(ImageSharpTexture imageTexture, Veldrid.Texture deviceTexture, TextureView textureView, long hash)
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
            if (String.IsNullOrEmpty(executablePath))
                executablePath = AppDomain.CurrentDomain.BaseDirectory;
            if (String.IsNullOrEmpty(executablePath))
                throw new Exception("Executable path cannot be null or empty");
            var executableDirectory = Path.GetDirectoryName(executablePath);
            path = Path.Combine(executableDirectory, path);
            Console.WriteLine($"Loading texture at {path}");
            if (File.Exists(path))
            {
                var texture = new ImageSharpTexture(path);
                var deviceTexture = texture.CreateDeviceTexture(graphicsDevice, resourceFactory);
                var textureView = resourceFactory.CreateTextureView(deviceTexture);
                var hash = 0l;
                using (var sha = SHA256Managed.Create())
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var rawHash = sha.ComputeHash(stream);
                        hash = BitConverter.ToInt64(rawHash, 0);
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
            _deviceTexture?.Dispose();
            _textureView?.Dispose();
        }
    }
}