using System.Collections.Generic;

namespace Neptune.Engine
{
    public interface IRenderable
    {
        (List<VertexInfo>, List<ushort>) GetTriangles();
    }
}