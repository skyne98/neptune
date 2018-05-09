using System.Collections.Generic;

namespace Neptune.Engine
{
    public interface ISpriteBatch
    {
        void Draw(IRenderable renderable);
        (List<VertexInfo>, List<ushort>) GetTriangles();
    }
}