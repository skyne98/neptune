using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Neptune.Core.Engine.Sample
{
    public class SimpleGame: IGame
    {
        private readonly GraphicsDevice _graphicsDevice;
        private List<IRenderable> _sprites;
        private IRenderer _renderer;

        private List<IRenderable> _renderables;
        private Random _random;
        
        public SimpleGame(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _sprites = new List<IRenderable>();
            _renderer = new SimpleRenderer(_graphicsDevice);
            _renderables = new List<IRenderable>();
            
            _renderables.Add(new StupidRenderable(_graphicsDevice, new Vector2(500, 500), Vector2.One / 2));
            _renderables.Add(new StupidRenderable(_graphicsDevice, new Vector2(500, 500), Vector2.One / 2)
            {
                Rotation = 3.14159f
            });
            
            _renderables.Add(new StupidRenderable(_graphicsDevice, new Vector2(200, 600), Vector2.One / 3)
            {
                Origin = Vector2.One / 2
            });
            _renderables.Add(new StupidRenderable(_graphicsDevice, new Vector2(200, 800), Vector2.One / 3)
            {
                Origin = Vector2.One / 2
            });
            
            _random = new Random();
        }
        
        public void Update(UpdateInfo info)
        {
            foreach (StupidRenderable renderable in _renderables)
            {
                renderable.Rotation += 0.005f;
            }
        }

        public void DisposeResources()
        {
            _renderer.DisposeResources();
        }

        public void Render(RenderInfo info)
        {
            _renderer.BatchDraw(batch =>
            {
                foreach (var renderable in _renderables)
                {
                    batch.Draw(renderable);
                }
            });
        }

        public IRenderer GetRenderer()
        {
            return _renderer;
        }
    }
}