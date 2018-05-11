using System.Collections.Generic;

namespace Neptune.Core.Engine
{
    public interface ISpriteBatch
    {
        void Draw(IRenderable renderable);
        (List<VertexInfo>, List<ushort>) GetTriangles();
    }
}