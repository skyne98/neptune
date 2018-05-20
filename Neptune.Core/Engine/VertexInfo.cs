using System.Numerics;
using Veldrid;

namespace Neptune.Core.Engine
{
    public struct VertexInfo
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public Vector2 TexCoord;
        public RgbaFloat Color; // This is the color of the vertex.

        public VertexInfo(Vector2 position, Vector2 texCoord, RgbaFloat color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
        public const uint SizeInBytes = 32;
    }
}