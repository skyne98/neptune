using System;
using System.Collections.Generic;
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
    public class Program
    {
        static void Main()
        {
            var createInfo = new EngineWindowCreateInfo()
            {
                PreferredBackend = GraphicsBackend.Vulkan
            };
            var myWindow = new MyWindow(createInfo);
            myWindow.Run();
        }
    }

    public class MyWindow : EngineWindow
    {
        private List<SpritePrimitive> sprites = new List<SpritePrimitive>();

        public MyWindow(EngineWindowCreateInfo createInfo): base(createInfo)
        {
            // Resources
            ResourceManager.LoadTexture("Assets/doge.jpg", "Doge");
            var doge = ResourceManager.GetTexture("Doge");

            // Sprites
            var sprite = new SpritePrimitive(doge, GraphicsDevice)
            {
                Position = Vector2.Zero,
                ZIndex = 1f
            };
            var sprite2 = new SpritePrimitive(doge, GraphicsDevice)
            {
                Position = Vector2.Zero - doge.Size / 2,
                ZIndex = 0.5f
            };

            sprites.Add(sprite);
            sprites.Add(sprite2);
        }

        public override void Loop(EngineLoopInfo loopInfo)
        {
            foreach (var sprite in sprites)
            {
                Add(sprite);
            }

            sprites[0].ZIndex = (float)Math.Max(0d, Math.Cos(loopInfo.GlobalTime));
        }
    }
}