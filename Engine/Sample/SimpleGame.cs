using System.Collections.Generic;

namespace Neptune.Engine
{
    public class SimpleGame: IGame
    {
        private List<IRenderable> _sprites;
        private IRenderer _renderer;
        
        public SimpleGame()
        {
            _sprites = new List<IRenderable>();
            _renderer = new Renderer();
        }
        
        public void Update(UpdateInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void Render(RenderInfo info)
        {
            throw new System.NotImplementedException();
        }

        public IRenderer GetRenderer()
        {
            return _renderer;
        }
    }
}