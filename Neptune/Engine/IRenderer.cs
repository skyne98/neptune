using System;
using Neptune.Engine;

namespace Neptune.Engine
{
    public interface IRenderer
    {
        void BatchDraw(Action<ISpriteBatch> action);
        void DisposeResources();
    }
}