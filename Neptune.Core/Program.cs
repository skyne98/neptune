using System;
using System.Threading;
using Neptune.Core.Engine.Sample;
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
            var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGL);
            var game = new SimpleGame(graphicsDevice);
            window.Resized += () => {
                graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            };
            
            Console.WriteLine($"Backend: {graphicsDevice.BackendType}");

            while (window.Exists)
            {
                window.PumpEvents();
                game.Update(null);
                game.Render(null);

                Thread.Sleep(1);
            }
            
            game.DisposeResources();
        }
    }
}