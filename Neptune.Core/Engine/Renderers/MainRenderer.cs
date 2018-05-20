using System;
using System.Collections.Generic;
using System.Threading;
using Neptune.Core.Engine.Primitives;
using Veldrid;
using Veldrid.Sdl2;
using Vulkan;

namespace Neptune.Core.Engine.Renderers
{
    public class MainRenderer: IDisposable
    {
        public FrameTimeAverager FrameTimeAverager
        {
            get => _frameTimeAverager;
        }
        public float GlobalTime
        {
            get => _time;
        }
        public float LastFrameTimeMilliseconds
        {
            get => _lastFrameTimeMilliseconds;
        }
        public float LastFrameTimeSeconds
        {
            get => _lastFrameTimeMilliseconds * 1000f;
        }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _resourceFactory;
        private readonly CommandList _commandList;
        private readonly Sdl2Window _window;
        private readonly ImGuiRenderer _guiRenderer;

        private FrameTimeAverager _frameTimeAverager;
        private float _time = 0.0f;
        private DateTime _lastFrame = DateTime.Now;
        private float _lastFrameTimeMilliseconds = 0f;
        private float _fpsCap = 0.0f;

        private SpritePrimitiveRenderer _spritePrimitiveRenderer;
        
        public MainRenderer(MainRendererCreateInfo createInfo)
        {
            _graphicsDevice = createInfo.GraphicsDevice;
            _resourceFactory = createInfo.GraphicsDevice.ResourceFactory;
            _commandList = _resourceFactory.CreateCommandList();
            _window = createInfo.Window;

            _frameTimeAverager = new FrameTimeAverager(0.222f);
            _fpsCap = createInfo.FramesPerSecondCap;

            _spritePrimitiveRenderer = new SpritePrimitiveRenderer(_graphicsDevice, _commandList, _window);
        }

        public void Add(IRenderingPrimitive renderingPrimitive)
        {
            switch (renderingPrimitive)
            {
                case SpritePrimitive sprite:
                    _spritePrimitiveRenderer.Add(sprite);
                    break;
            }
        }

        public void Render(ImGuiRenderer imGuiRenderer)
        {
            var frameTime = DateTime.Now - _lastFrame;
            _time += (float)frameTime.TotalSeconds;
            _frameTimeAverager.AddTime(frameTime.TotalSeconds);
            _lastFrameTimeMilliseconds = frameTime.Milliseconds;
            _lastFrame = DateTime.Now;

            if (_frameTimeAverager.CurrentAverageFramesPerSecond > _fpsCap)
            {
                Thread.Sleep(Math.Max(0, (1000 / (int)_fpsCap) - (int)_frameTimeAverager.CurrentAverageFrameTimeMilliseconds));
            }

            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(float.MaxValue);

            _spritePrimitiveRenderer.Render();
            imGuiRenderer.Render(_graphicsDevice, _commandList);
            
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            if (_window.Exists)
            {
                _graphicsDevice.SwapBuffers();
                _graphicsDevice.WaitForIdle();
            }
        }

        public void Dispose()
        {
            _spritePrimitiveRenderer?.Dispose();
            
            _graphicsDevice?.Dispose();
            _commandList?.Dispose();
        }
    }
}