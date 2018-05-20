using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Neptune.Core.Engine
{
    public class EngineWindowCreateInfo
    {
        // Window
        public string Title { get; set; } = "Untitled Window";
        public Vector2 Position { get; set; } = new Vector2(100.0f, 100.0f);
        public Vector2 Size { get; set; } = new Vector2(960.0f, 540.0f);

        // Graphics device
        public GraphicsBackend PreferredBackend { get; set; } = Veldrid.StartupUtilities.VeldridStartup.GetPlatformDefaultBackend();
        public List<GraphicsBackend> FallbackBackends { get; set; } = new List<GraphicsBackend>() { GraphicsBackend.OpenGL };

        // Engine
        public float FramesPerSecondCap { get; set; } = 120.0f;
    }
}
