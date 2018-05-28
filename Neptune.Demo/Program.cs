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
//            ParallelNative.TransformNative transformNative = new ParallelNative.TransformNative()
//            {
//                X = 20,
//                Y = 30,
//                Rotation = 45,
//                SizeX = 2,
//                SizeY = 3
//            };
//            var matrix = Matrix4x4.Identity;
//            var transforms = new ParallelNative.TransformNative[] {transformNative};
//            var matrices = new Matrix4x4[] {matrix};
//            ParallelNative.CalculateMatrices(transforms, matrices);
//            Console.WriteLine($"Got: {matrices[0]}");
//            var modelMatrix = Matrix4x4.CreateScale(new Vector3(new Vector2(2, 3), 1.0f));
//            modelMatrix = modelMatrix * Matrix4x4.CreateTranslation(new Vector3(new Vector2(20, 30), 0));
//            Console.WriteLine($"Needed: {modelMatrix}");
//            JobManager jobManager = new JobManager();
//            List<RecurrentJob> jobs = new List<RecurrentJob>();
//            for (int i = 0; i < 1000000; i++)
//            {
//                var job = new MyJob();
//                jobs.Add(job);
//            }
//
//            Benchmark(() =>
//            {
//                foreach (var job in jobs)
//                {
//                    job.Schedule(jobManager);
//                }
//                jobManager.WaitAll();
//            });
//            foreach (var job in jobs)
//            {
//                job.Reset();
//            }
//            Benchmark(() =>
//            {
//                foreach (var job in jobs)
//                {
//                    job.Execute();
//                }
//            });
//            
//            Benchmark(() =>
//            {
//                foreach (var job in jobs)
//                {
//                    job.Reset();
//                }
//                jobManager.Reset();
//                foreach (var job in jobs)
//                {
//                    job.Schedule(jobManager);
//                }
//                jobManager.WaitAll();
//            });
//
//            Benchmark((() =>
//            {
//                for (int i = 0; i < 1000000; i++)
//                {
//                    var sum = Matrix4x4.Identity;
//                    for (int x = 0; x < 4; x++)
//                    {
//                        sum *= Matrix4x4.CreateRotationX(30); ;
//                    }
//                }
//            }));
//            var parallelNative = new ParallelNative();
//
//            Console.ReadLine();
        }

        public static void Benchmark(Action action)
        {
            var startMemory = GC.GetTotalMemory(true);
            var startTime = DateTime.Now;
            action();
            var resultMemory = GC.GetTotalMemory(false);
            var resultTime = DateTime.Now;
            Console.WriteLine($"Time: {(resultTime - startTime).TotalMilliseconds} ms Memory: {resultMemory - startMemory} bytes / {(resultMemory - startMemory) / 1024} kbytes");
        }
    }

    public class MyJob : RecurrentJob
    {
        public List<RecurrentJob> Dependencies = new List<RecurrentJob>();

        public override List<RecurrentJob> GetDependencies()
        {
            return Dependencies;
        }

        public override void Execute()
        {
            var sum = Matrix4x4.Identity;
            for (int i = 0; i < 4; i++)
            {
                sum *= Matrix4x4.CreateRotationX(30);;
            }
        }
    }

    public class MyChildJob : RecurrentJob
    {
        private readonly RecurrentJob _parent;

        public MyChildJob(RecurrentJob parent)
        {
            _parent = parent;
        }

        public override List<RecurrentJob> GetDependencies()
        {
            return new List<RecurrentJob>()
            {
                _parent
            };
        }

        public override void Execute()
        {
            Console.WriteLine($"Child job executing at {Thread.CurrentThread.ManagedThreadId}");
        }
    }

    public class MyWindow : EngineWindow
    {
        private List<SpritePrimitive> sprites = new List<SpritePrimitive>();
        private int doges = 1;

        public MyWindow(EngineWindowCreateInfo createInfo) : base(createInfo)
        {
            // Resources
            ResourceManager.LoadTexture("Assets/doge.jpg", "Doge");
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
                    int max = 100000;
                    if (ImGui.SliderInt("Doges", ref doges, min, max, "Amount"))
                    {
                        Console.WriteLine($"You chose {doges} doges");

                        var toAdd = Math.Max(0, doges - sprites.Count);
                        Console.WriteLine($"Adding {toAdd}");
                        var doge = ResourceManager.GetTexture("Doge");

                        for (int i = 0; i < toAdd; i++)
                        {
                            // Sprites
                            var random = new Random();
                            var next = random.NextDouble();
                            var sprite = new SpritePrimitive(doge, GraphicsDevice)
                            {
                                Position = new Vector2((float)random.NextDouble() * 200, (float)random.NextDouble() * 300)
                            };
                            sprites.Add(sprite);
                            Add(sprite);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }


            //sprites[0].ZIndex = (float)Math.Max(0d, Math.Cos(loopInfo.GlobalTime));
            //sprites[0].Rotation += 15f * loopInfo.SecondsPerFrame;
            //sprites[1].Rotation += 30f * loopInfo.SecondsPerFrame;
            Console.Title = Math.Max(0d, Math.Cos(loopInfo.GlobalTime)).ToString();
        }

        private unsafe int OnTextEdited(TextEditCallbackData* data)
        {
            char currentEventChar = (char)data->EventChar;
            Console.WriteLine($"Char: {currentEventChar} Stirng: {""}");
            return 0;
        }
    }
}
