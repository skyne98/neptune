using System.Collections.Generic;

namespace Neptune.Core.Engine.Renderers
{
    public interface IRenderer<T>
    {
        void Render(List<T> primitives);
    }
}