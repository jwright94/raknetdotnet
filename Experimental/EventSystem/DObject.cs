namespace EventSystem
{
    internal delegate void HandleEventDelegate(IEvent e);

    internal class DObject : IDObject
    {
        private readonly IDOManager manager;
        private int oId;

        public HandleEventDelegate OnGetEvent;

        public DObject(IDOManager manager)
        {
            this.manager = manager;
        }

        public int OId
        {
            get { return oId; }
            set { oId = value; }
        }

        public void HandleEvent(IEvent e)
        {
            if (OnGetEvent != null)
            {
                OnGetEvent(e);
            }
        }

        public void PostEvent(IEvent e)
        {
            manager.PostEvent(e);
        }

        public void SendEvent(IEvent e)
        {
            manager.SendEvent(e);
        }
    }
}