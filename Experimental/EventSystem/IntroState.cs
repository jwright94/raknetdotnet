using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using System.Diagnostics;

    sealed class IntroState : AbstractGameState, IDisposable
    {
        #region Ogre-like singleton implementation.
        static IntroState instance;
        public IntroState()
            : base("Intro")
        {
            Debug.Assert(instance == null);
            instance = this;
        }
        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;
        }
        public static IntroState Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }
        #endregion
        #region IGameState Members
        public override void Enter()
        {
        }
        public override void Exit()
        {
        }
        public override void Pause()
        {
        }
        public override void Resume()
        {
        }
        public override bool KeyPressed(char key)
        {
            if (key == 'p')
            {
                ChangeState(PlayState.Instance);
            }
            return true;
        }
        public override bool FrameStarted()
        {
            return true;
        }
        #endregion
    }
}
