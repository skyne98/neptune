using System;
using Neptune.Engine;

interface IRenderer
{
    void BatchDraw(Action<ISpriteBatch> action);
}