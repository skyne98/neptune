namespace Neptune.Core.Engine
{
    interface IGame
    {
        void Update(UpdateInfo info);
        void Render(RenderInfo info);

        void DisposeResources();
    
        IRenderer GetRenderer();
    }
}