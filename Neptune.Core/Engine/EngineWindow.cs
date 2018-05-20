using Neptune.Core.Engine.Input;
using Neptune.Core.Engine.Primitives;
using Neptune.Core.Engine.Renderers;
using Neptune.Core.Engine.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Neptune.Core.Engine
{
    public abstract class EngineWindow
    {
        public ResourceManager ResourceManager
        {
            get => _resourceManager;
        }
        public GraphicsDevice GraphicsDevice
        {
            get => _graphicsDevice;
        }

        private MainRenderer _renderer;
        private ResourceManager _resourceManager;
        private InputManager _inputManager;
        private GraphicsDevice _graphicsDevice;
        private Sdl2Window _window;

        private EngineLoopInfo _loopInfo;
        private FrameTimeAverager _frameTimeAverager;
        private float _time = 0.0f;
        private DateTime _lastFrame = DateTime.Now;
        private float _fpsCap = 0.0f;

        public EngineWindow(EngineWindowCreateInfo createInfo)
        {
            CreateWindow(createInfo);
            CreateGraphicsDevice(createInfo);
            CreateRenderer(createInfo);
            CreateResourceManager(createInfo);
            CreateInputManager(createInfo);

            _window.Resized += () => {
                _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
            };
            _fpsCap = createInfo.FramesPerSecondCap;

            var initializedInfo = new EngineInitializedInfo()
            {
                CurrentGraphicsBackend = _graphicsDevice.BackendType
            };
        }
        
        public abstract void Loop(EngineLoopInfo loopInfo);

        public void Run()
        {
            _frameTimeAverager = new FrameTimeAverager(0.222f);
            _loopInfo = new EngineLoopInfo()
            {
                FramesPerSecond = 0,
                GlobalTime = _time,
                MillisecondsPerFrame = 0,
                SecondsPerFrame = 0,
            };
            while (_window.Exists)
            {
                _window.PumpEvents();
                _loopInfo.FramesPerSecond = (float)_frameTimeAverager.CurrentAverageFramesPerSecond;
                _loopInfo.GlobalTime = _time;
                _loopInfo.MillisecondsPerFrame = (float)_frameTimeAverager.CurrentAverageFrameTimeMilliseconds;
                _loopInfo.SecondsPerFrame = (float)_frameTimeAverager.CurrentAverageFrameTimeSeconds;
                Loop(_loopInfo);
                _renderer.Render();

                var frameTime = DateTime.Now - _lastFrame;
                _time += (float)frameTime.TotalSeconds;
                _frameTimeAverager.AddTime(frameTime.TotalSeconds);
                _lastFrame = DateTime.Now;

                if (_frameTimeAverager.CurrentAverageFramesPerSecond > _fpsCap)
                {
                    Thread.Sleep(Math.Max(0, (1000 / (int)_fpsCap) - (int)_frameTimeAverager.CurrentAverageFrameTimeMilliseconds));
                }
            }

            _renderer.Dispose();
        }

        public void Add(IRenderingPrimitive primitive)
        {
            _renderer.Add(primitive);
        }

        private void CreateWindow(EngineWindowCreateInfo createInfo)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = (int)createInfo.Position.X,
                Y = (int)createInfo.Position.Y,
                WindowWidth = (int)createInfo.Size.X,
                WindowHeight = (int)createInfo.Size.Y,
                WindowTitle = createInfo.Title
            };
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);
            _window = window;
        }
        private void CreateGraphicsDevice(EngineWindowCreateInfo createInfo)
        {
            var graphicsDeviceOptions = new GraphicsDeviceOptions()
            {
                SwapchainDepthFormat = PixelFormat.R16_UNorm,
                SyncToVerticalBlank = true,
                ResourceBindingModel = ResourceBindingModel.Improved
            };

            try
            {
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions, createInfo.PreferredBackend);
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Failed to initialize the preferred {createInfo.PreferredBackend} backend: {Environment.NewLine} {ex}");

                if (createInfo.FallbackBackends.Count > 0)
                {
                    Console.WriteLine($"Trying fallbacks...");
                }
                foreach (var fallback in createInfo.FallbackBackends)
                {
                    try
                    {
                        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions, fallback);
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (_graphicsDevice == null)
                {
                    throw new Exception("Unable to initialize a graphics device");
                }
            }

            Console.WriteLine($"{_graphicsDevice.BackendType} device was initialized");
        }
        private void CreateRenderer(EngineWindowCreateInfo createInfo)
        {
            var renderer = new MainRenderer(_graphicsDevice, _window);
            _renderer = renderer;
        }
        private void CreateResourceManager(EngineWindowCreateInfo createInfo)
        {
            var resourceManager = new ResourceManager(_graphicsDevice);

            // Load the default textures
            resourceManager.LoadTexture("Assets/MissingTexture.png", "__MissingTexture");

            _resourceManager = resourceManager;
        }
        private void CreateInputManager(EngineWindowCreateInfo createInfo)
        {
            var inputManager = new InputManager(_window);

            _inputManager = inputManager;
        }
    }
}
