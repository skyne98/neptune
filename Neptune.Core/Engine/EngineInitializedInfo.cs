using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace Neptune.Core.Engine
{
    public class EngineInitializedInfo
    {
        public GraphicsBackend CurrentGraphicsBackend { get; internal set; }
    }
}
