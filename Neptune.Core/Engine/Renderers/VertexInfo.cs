using System.Numerics;
using Veldrid;

namespace Neptune.Core.Engine.Renderers
{
    public struct VertexInfo
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public Vector2 TexCoord;

        public VertexInfo(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
        public const uint SizeInBytes = 16;
    }
}