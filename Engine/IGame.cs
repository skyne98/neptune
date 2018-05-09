using Neptune.Engine;

interface IGame
{
    void Update(UpdateInfo info);
    void Render(RenderInfo info);
    
    IRenderer GetRenderer();
}