namespace EventSystem
{
    internal abstract class AbstractGameState : IGameState
    {
        public AbstractGameState(string _name)
        {
            name = _name;
        }

        public virtual string Name
        {
            get { return name; }
        }

        private string name;

        #region Protected Members

        protected void ChangeState(IGameState state)
        {
            GameManager.Instance.ChangeState(state);
        }

        protected void PushState(IGameState state)
        {
            GameManager.Instance.PushState(state);
        }

        protected void PopState()
        {
            GameManager.Instance.PopState();
        }

        #endregion

        #region IGameState Members

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Pause();
        public abstract void Resume();
        public abstract bool KeyPressed(char key);
        public abstract bool FrameStarted();

        #endregion
    }
}