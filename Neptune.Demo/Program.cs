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
                PreferredBackend = GraphicsBackend.Vulkan
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
                ImGui.Text($"{loopInfo.FramesPerSecond} fps / {loopInfo.MillisecondsPerFrame} ms");

                if (ImGui.BeginMenu("Options"))
                {
                    int min = 0;
                    int max = 1000000;
                    if (ImGui.SliderInt("Doges", ref doges, min, max, "Amount"))
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
                                Position = new Vector2((float)random.NextDouble() * 600, (float)random.NextDouble() * 600)
                            };
                            sprites.Add(sprite);
                            Add(sprite);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        private unsafe int OnTextEdited(TextEditCallbackData* data)
        {
            char currentEventChar = (char)data->EventChar;
            Console.WriteLine($"Char: {currentEventChar} Stirng: {""}");
            return 0;
        }
    }
}
