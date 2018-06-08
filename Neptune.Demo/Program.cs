using ImGuiNET;
using Neptune.Core.Engine;
using Neptune.Core.Engine.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Neptune.JobSystem;
using Neptune.JobSystem.Jobs;
using Neptune.JobSystem.Native;
using Veldrid;

namespace Neptune.Demo
{
    public class Program
    {
        static void Main()
        {
            var createInfo = new EngineWindowCreateInfo()
            {
                PreferredBackend = GraphicsBackend.OpenGL
            };
            var myWindow = new MyWindow(createInfo);
            myWindow.Run();
        }
    }

    public class MyWindow : EngineWindow
    {
        private List<SpritePrimitive> sprites = new List<SpritePrimitive>();
        private int doges = 1;

        public MyWindow(EngineWindowCreateInfo createInfo) : base(createInfo)
        {
            // Resources
            ResourceManager.LoadTexture("Assets/Doge.jpg", "Doge");
            ResourceManager.LoadTexture("Assets/Rabbit.png", "Bunny");
        }

        public unsafe override void Loop(EngineLoopInfo loopInfo)
        {
            var style = ImGui.GetStyle();
            style.SetColor(ColorTarget.FrameBg, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            ImGui.StyleColorsClassic(style);

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    int min = 0;
                    int max = 1000000;
                    if (ImGui.SliderInt("Doges", ref doges, min, max, doges.ToString()))
                    {
                        Console.WriteLine($"You chose {doges} doges");

                        var toAdd = Math.Max(0, doges - sprites.Count);
                        Console.WriteLine($"Adding {toAdd}");
                        var texture = ResourceManager.GetTexture("Bunny");

                        for (int i = 0; i < toAdd; i++)
                        {
                            // Sprites
                            var random = new Random();
                            var next = random.NextDouble();
                            var sprite = new SpritePrimitive(texture)
                            {
                                Position = new Vector2((float)random.NextDouble() * 600, (float)random.NextDouble() * 600),
                                Origin = Vector2.One / 2
                            };
                            //sprite.ZIndex = sprite.Position.X / 10000f;
                            sprites.Add(sprite);
                            Add(sprite);
                        }
                    }

                    ImGui.EndMenu();
                }
                ImGui.Separator();
                ImGui.Text($"{loopInfo.FramesPerSecond} fps / {loopInfo.MillisecondsPerFrame} ms");
                ImGui.EndMainMenuBar();
            }

            foreach (var spritePrimitive in sprites)
            {
                spritePrimitive.Rotation += 3.0f * loopInfo.SecondsPerFrame;
            }
        }
    }
}
