using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Neptune.Core.Engine.Renderers
{
    public class InstanceInfo
    {
        public Vector4 TransformColumn0;
        public Vector4 TransformColumn1;
        public Vector4 TransformColumn2;
        public Vector4 TransformColumn3;
        public float ZIndex;
        public Vector4 Color;

        public InstanceInfo(Vector4 transformColumn0, Vector4 transformColumn1, Vector4 transformColumn2, Vector4 transformColumn3, float zIndex, RgbaFloat color)
        {
            TransformColumn0 = transformColumn0;
            TransformColumn1 = transformColumn1;
            TransformColumn2 = transformColumn2;
            TransformColumn3 = transformColumn3;
            ZIndex = zIndex;
            Color = new Vector4(color.R, color.G, color.B, color.A);
        }
        public const uint SizeInBytes = 84;
    }
}
