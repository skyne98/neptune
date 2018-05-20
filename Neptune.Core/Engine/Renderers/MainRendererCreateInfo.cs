using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;

namespace Neptune.Core.Engine.Renderers
{
    public class MainRendererCreateInfo
    {
        // Graphics device
        public GraphicsDevice GraphicsDevice { get; set; }

        // Window
        public Sdl2Window Window { get; set; }

        // Engine
        public float FramesPerSecondCap { get; set; } = 120.0f;
    }
}
