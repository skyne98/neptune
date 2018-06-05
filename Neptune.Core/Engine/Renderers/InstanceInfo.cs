using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Neptune.Core.Engine.Renderers
{
    public struct InstanceInfo
    {
        public float ZIndex;
        public int MatrixIndex;
        public Vector4 Color;

        public InstanceInfo(float zIndex, int matrixIndex, RgbaFloat color)
        {
            ZIndex = zIndex;
            MatrixIndex = matrixIndex;
            Color = new Vector4(color.R, color.G, color.B, color.A);
        }
        public const uint SizeInBytes = 4 + 4 + 16;
    }
}
