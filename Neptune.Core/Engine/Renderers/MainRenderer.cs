using System;
using System.Collections.Generic;
using ImGuiNET;
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
        private readonly ImGuiRenderer _imGuiRenderer;

        private SpritePrimitiveRenderer _spritePrimitiveRenderer;
        
        public MainRenderer(GraphicsDevice graphicsDevice, Sdl2Window window)
        {
            _graphicsDevice = graphicsDevice;
            _resourceFactory = graphicsDevice.ResourceFactory;
            _commandList = _resourceFactory.CreateCommandList();
            _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _graphicsDevice.SwapchainFramebuffer.OutputDescription, window.Width, window.Height);
            window.Resized += () =>
            {
                _imGuiRenderer.WindowResized(window.Width, window.Height);
            };
            
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

        public void Render(InputSnapshot inputSnapshot, float frameTime)
        {
            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(float.MaxValue);

            _spritePrimitiveRenderer.Render();
            RenderMenu(inputSnapshot, frameTime);
            
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers();
            _graphicsDevice.WaitForIdle();
        }

        public void RenderMenu(InputSnapshot inputSnapshot, float frameTime)
        {
            _imGuiRenderer.Update(frameTime, inputSnapshot);
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Settings"))
                {
                    if (ImGui.BeginMenu("Graphics Backend"))
                    {
                        if (ImGui.MenuItem("Vulkan", GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)))
                        {

                        }
                        if (ImGui.MenuItem("OpenGL", GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL)))
                        {

                        }
                        if (ImGui.MenuItem("OpenGL ES", GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES)))
                        {

                        }
                        if (ImGui.MenuItem("Direct3D 11", GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11)))
                        {

                        }
                        if (ImGui.MenuItem("Metal", GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal)))
                        {

                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
            _imGuiRenderer.Render(_graphicsDevice, _commandList);
        }

        public void Dispose()
        {
            _spritePrimitiveRenderer?.Dispose();
            
            _graphicsDevice?.Dispose();
            _commandList?.Dispose();
        }
    }
}