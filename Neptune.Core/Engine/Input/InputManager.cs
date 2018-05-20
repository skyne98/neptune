using System;
using System.Collections.Generic;
using System.Text;
using Veldrid.Sdl2;

namespace Neptune.Core.Engine.Input
{
    public class InputManager
    {
        private Sdl2Window _window;

        public InputManager(Sdl2Window window)
        {
            _window = window;
        }
    }
}