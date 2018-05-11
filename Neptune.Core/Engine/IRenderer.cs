using System;

namespace Neptune.Core.Engine
{
    public interface IRenderer
    {
        void BatchDraw(Action<ISpriteBatch> action);
        void DisposeResources();
    }
}