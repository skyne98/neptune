using System.Collections.Generic;

namespace Neptune.Core.Engine
{
    public interface IRenderable
    {
        (List<VertexInfo>, List<ushort>) GetTriangles();
    }
}