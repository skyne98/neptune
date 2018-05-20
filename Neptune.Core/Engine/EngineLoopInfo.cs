using System;
using System.Collections.Generic;
using System.Text;

namespace Neptune.Core.Engine
{
    public class EngineLoopInfo
    {
        public float GlobalTime { get; internal set; }
        public float FramesPerSecond { get; internal set; }
        public float SecondsPerFrame { get; internal set; }
        public float MillisecondsPerFrame { get; internal set; }
    }
}
