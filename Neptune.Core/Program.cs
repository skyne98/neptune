using System;
using System.Numerics;
using System.Threading;
using Neptune.Core.Engine;
using Neptune.Core.Engine.Primitives;
using Neptune.Core.Engine.Renderers;
using Neptune.Core.Engine.Resources;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Neptune.Core
{
    class Program
    {
        static void Main()
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Tutorial"
            };
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);
            GraphicsDevice graphicsDevice = null;
            try
            {
                graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGLES);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Failed to initialize Vulkan, using OpenGL fallback");

                graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGL);
            }
            var renderer = new MainRenderer(graphicsDevice, window);
            window.Resized += () => {
                graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            };

            Console.WriteLine($"Backend: {graphicsDevice.BackendType}");

            // Load texture
            var texture = Texture2D.FromFile("Assets/doge.jpg", graphicsDevice, graphicsDevice.ResourceFactory);
            
            // Create a sprite
            var sprite = new SpriteRenderingPrimitive(texture, graphicsDevice);
            sprite.Origin = Vector2.One / 2;
            sprite.Rotation = 0f;
            sprite.Scale = Vector2.One;
            sprite.Position = Vector2.Zero;
            
            // Create a sprite2
            var sprite2 = new SpriteRenderingPrimitive(texture, graphicsDevice);
            sprite2.Origin = Vector2.One / 2;
            sprite2.Rotation = 0f;
            sprite2.Scale = Vector2.One / 10;
            sprite2.Position = Vector2.Zero;

            // Frame averager
            var frameAverager = new FrameTimeAverager(0.666f);
            var lastFrame = DateTime.Now;

            var time = 0.0f;
            while (window.Exists)
            {
                window.PumpEvents();

                sprite.Rotation = time * (float)Math.PI / 2;
                sprite2.Scale = Vector2.One / 10 * (float)Math.Cos(time);
                
                renderer.Add(sprite);
                renderer.Add(sprite2);
                
                renderer.Render();

                var frameTime = DateTime.Now - lastFrame;
                time += (float)frameTime.TotalSeconds;
                frameAverager.AddTime(frameTime.TotalSeconds);
                lastFrame = DateTime.Now;
                window.Title = $"Neptune Demo: {Math.Round(frameAverager.CurrentAverageFramesPerSecond)} fps";
                Thread.Sleep(1);
            }

            renderer.Dispose();
        }
    }
}