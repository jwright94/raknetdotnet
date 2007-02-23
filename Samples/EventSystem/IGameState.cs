using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    interface IGameState
    {
        string Name { get; }
        void Enter();
        void Exit();
        void Pause();
        void Resume();
        bool KeyPressed(char key);
        bool FrameStarted();
    }
}
