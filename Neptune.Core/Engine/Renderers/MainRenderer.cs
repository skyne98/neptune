using System;
using System.Collections.Generic;
using Neptune.Core.Engine.Primitives;
using Veldrid;
using Veldrid.Sdl2;
using Vulkan;

namespace Neptune.Core.Engine.Renderers
{
    public class MainRenderer: IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _resourceFactory;
        private readonly CommandList _commandList;

        private SpritePrimitiveRenderer _spritePrimitiveRenderer;
        
        public MainRenderer(GraphicsDevice graphicsDevice, Sdl2Window window)
        {
            _graphicsDevice = graphicsDevice;
            _resourceFactory = graphicsDevice.ResourceFactory;
            _commandList = _resourceFactory.CreateCommandList();
            
            _spritePrimitiveRenderer = new SpritePrimitiveRenderer(graphicsDevice, _commandList, window);
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

        public void Render()
        {
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(float.MaxValue);

            _spritePrimitiveRenderer.Render();
            
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
            _graphicsDevice.WaitForIdle();
        }

        public void Dispose()
        {
            _spritePrimitiveRenderer?.Dispose();
            
            _graphicsDevice?.Dispose();
            _commandList?.Dispose();
        }
    }
}