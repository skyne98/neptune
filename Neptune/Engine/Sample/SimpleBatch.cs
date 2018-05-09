using System.Collections.Generic;
using System.Linq;

namespace Neptune.Engine.Sample
{
    public class SimpleBatch: ISpriteBatch
    {
        private List<IRenderable> _renderables;

        public SimpleBatch()
        {
            _renderables = new List<IRenderable>();
        }
                
        public void Draw(IRenderable renderable)
        {
            _renderables.Add(renderable);
        }

        public (List<VertexInfo>, List<ushort>) GetTriangles()
        {
            List<VertexInfo> newVertices = new List<VertexInfo>();
            List<ushort> newTriangles = new List<ushort>();

            for (var i = 0; i < _renderables.Count; i++)
            {
                var renderable = _renderables[i];
                var (vertices, triangles) = renderable.GetTriangles();
                newVertices.AddRange(vertices);

                var offset = (ushort)triangles.Count * i;
                newTriangles.AddRange(triangles.Select(t => (ushort)(t)));
            }

            return (newVertices, newTriangles);
        }
    }
}