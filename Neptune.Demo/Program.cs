using ImGuiNET;
using Neptune.Core.Engine;
using Neptune.Core.Engine.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Neptune.JobSystem;
using Neptune.JobSystem.Jobs;
using Veldrid;

namespace Neptune.Demo
{
    public class Program
    {
        public static int Samples = 1000000;
        public static int Iterations = 40;
        
        static void Main()
        {
            JobManager jobManager = new JobManager();

            var startTime = DateTime.Now;
            for (int i = 0; i < Samples; i++)
            {
                var job = new MyJob(jobManager);
                job.Schedule();
            }
            jobManager.WaitAll();
            Console.WriteLine($"Jobs {(DateTime.Now - startTime).TotalMilliseconds} ms");
            
            startTime = DateTime.Now;
            for (int i = 0; i < Samples; i++)
            {
                var sum = Matrix4x4.Identity;
            
                for (int x = 0; x < Iterations; x++)
                {
                    sum *= Matrix4x4.Identity;
                }
            }
            Console.WriteLine($"Sync {(DateTime.Now - startTime).TotalMilliseconds} ms");

            startTime = DateTime.Now;
            var taskFactory = new TaskFactory();
            var taskList = new List<Task>();
            for (int i = 0; i < Samples; i++)
            {
                var task = taskFactory.StartNew((() =>
                {
                    var sum = Matrix4x4.Identity;
            
                    for (int x = 0; x < Iterations; x++)
                    {
                        sum *= Matrix4x4.Identity;
                    }
                }));
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());
            Console.WriteLine($"Tasks {(DateTime.Now - startTime).TotalMilliseconds} ms");
//            var createInfo = new EngineWindowCreateInfo()
//            {
//                PreferredBackend = GraphicsBackend.OpenGL
//            };
//            var myWindow = new MyWindow(createInfo);
//            myWindow.Run();
        }
    }

    public class MyJob : Job
    {
        public MyJob(JobManager jobManager) : base(jobManager)
        {
        }

        public override void Execute()
        {
            var sum = Matrix4x4.Identity;
            
            for (int i = 0; i < Program.Iterations; i++)
            {
                sum *= Matrix4x4.Identity;
            }
        }

        public override List<Job> GetDependencies()
        {
            return null;
        }
    }

    public class MyWindow : EngineWindow
    {
        private List<SpritePrimitive> sprites = new List<SpritePrimitive>();

        public MyWindow(EngineWindowCreateInfo createInfo) : base(createInfo)
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
                Position = Vector2.Zero - doge.Get().Size / 2,
                ZIndex = 0.5f
            };

            sprites.Add(sprite);
            sprites.Add(sprite2);
        }

        public unsafe override void Loop(EngineLoopInfo loopInfo)
        {
            foreach (var sprite in sprites)
            {
                Add(sprite);
            }

            var style = ImGui.GetStyle();
            style.SetColor(ColorTarget.FrameBg, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            ImGui.StyleColorsClassic(style);            
            
            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text($"{loopInfo.FramesPerSecond} fps");

                if (ImGui.BeginMenu("Some stuff"))
                {
                    if (ImGui.MenuItem("Some item"))
                    {
                        Console.WriteLine("You clicked some item");
                    }
                    byte[] buffer = new byte[256];
                    if (ImGui.InputText("Label", buffer, 256, InputTextFlags.Default, OnTextEdited))
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    }
                    
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }


            sprites[0].ZIndex = (float)Math.Max(0d, Math.Cos(loopInfo.GlobalTime));
        }
        
        private unsafe int OnTextEdited(TextEditCallbackData* data)
        {
            char currentEventChar = (char)data->EventChar;
            Console.WriteLine($"Char: {currentEventChar} Stirng: {""}");
            return 0;
        } 
    }
}
