using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EventSystem
{
    internal sealed class GameManager : IDisposable, IKeyListener, IFrameListener
    {
        #region Ogre-like singleton implementation.

        private static GameManager instance;

        public GameManager()
        {
            Debug.Assert(instance == null);
            instance = this;
        }

        public void Dispose()
        {
            Debug.Assert(instance != null);
            instance = null;

            // clean up all the states
            while (0 < states.Count)
            {
                Debug.Assert(states.Peek() != null);
                states.Peek().Exit();
                states.Pop();
            }
        }

        public static GameManager Instance
        {
            get
            {
                Debug.Assert(instance != null);
                return instance;
            }
        }

        #endregion

        public void Start(IGameState state)
        {
            log("starting...");

            Root.Instance.AddFrameListener(this);

            Root.Instance.AddKeyListener(this);

            log("pusing first state...");
            PushState(state);

            Root.Instance.StartRendering();
        }

        public void ChangeState(IGameState state)
        {
            Debug.Assert(state != null);

            log("changing to state: {0}", state.Name);

            // cleanup the current state
            if (0 < states.Count)
            {
                IGameState oldState = states.Peek();

                if (state.Name == oldState.Name) return;

                log("exiting current state: {0}", oldState.Name);
                oldState.Exit();
                states.Pop();
            }

            // store and init the new state
            log("entering new state: {0}", state.Name);
            states.Push(state);
            states.Peek().Enter();
        }

        public void PushState(IGameState state)
        {
            Debug.Assert(state != null);

            log("pushing state: {0}", state.Name);

            if (0 < states.Count)
            {
                IGameState oldState = states.Peek();
                log("pause current state: {0}", oldState.Name);
                oldState.Pause();
            }

            log("store and init the new state: ", state.Name);
            states.Push(state);
            states.Peek().Enter();
        }

        public void PopState()
        {
            log("popping state");

            // TODO - impl.
        }

        #region Private Members

        private void log(string message)
        {
            Console.WriteLine(message);
        }

        private void log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }

        private Stack<IGameState> states = new Stack<IGameState>();

        #endregion

        #region IFrameListener Members

        public bool FrameStarted()
        {
            return states.Peek().FrameStarted();
        }

        #endregion

        #region IKeyListener Members

        public bool KeyPressed(char key)
        {
            states.Peek().KeyPressed(key);
            return true;
        }

        #endregion
    }
}