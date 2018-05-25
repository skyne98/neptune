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
        private ImGuiRenderer _imGuiRenderer;

        public EngineWindow(EngineWindowCreateInfo createInfo)
        {
            CreateWindow(createInfo);
            CreateGraphicsDevice(createInfo);
            CreateResourceManager(createInfo);
            CreateRenderer(createInfo);
            CreateInputManager(createInfo);
            CreateImGuiRenderer(createInfo);

            _window.Resized += () => {
                _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
            };

            var initializedInfo = new EngineInitializedInfo()
            {
                CurrentGraphicsBackend = _graphicsDevice.BackendType
            };
        }
        
        public abstract void Loop(EngineLoopInfo loopInfo);

        public void Run()
        {
            _loopInfo = new EngineLoopInfo()
            {
                FramesPerSecond = 0,
                GlobalTime = _renderer.GlobalTime,
                MillisecondsPerFrame = 0,
                SecondsPerFrame = 0,
            };
            while (_window.Exists)
            {
                var inputsSnapshot = _window.PumpEvents();
                _loopInfo.FramesPerSecond = (float)_renderer.FrameTimeAverager.CurrentAverageFramesPerSecond;
                _loopInfo.GlobalTime = _renderer.GlobalTime;
                _loopInfo.MillisecondsPerFrame = (float)_renderer.FrameTimeAverager.CurrentAverageFrameTimeMilliseconds;
                _loopInfo.SecondsPerFrame = (float)_renderer.FrameTimeAverager.CurrentAverageFrameTimeSeconds;
                _imGuiRenderer.Update(_renderer.LastFrameTimeSeconds, inputsSnapshot);
                Loop(_loopInfo);
                _renderer.Render(_imGuiRenderer);
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
            var mainRendererOptions = new MainRendererCreateInfo()
            {
                GraphicsDevice = _graphicsDevice,
                FramesPerSecondCap = createInfo.FramesPerSecondCap,
                Window = _window,
                ResourceManager = _resourceManager
            };
            var renderer = new MainRenderer(mainRendererOptions);
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
        private void CreateImGuiRenderer(EngineWindowCreateInfo createInfo)
        {
            _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _graphicsDevice.SwapchainFramebuffer.OutputDescription, _window.Width, _window.Height);
            _window.Resized += () =>
            {
                _imGuiRenderer.WindowResized(_window.Width, _window.Height);
            };
        }
    }
}
