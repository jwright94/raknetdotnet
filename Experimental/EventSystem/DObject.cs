namespace EventSystem
{
    public class DObject : IDObject
    {
        private readonly IDOManager manager;
        private int oId;
        public HandleEventDelegate OnGetEvent;


        public delegate void HandleEventDelegate(IEvent e);

        public DObject(IDOManager manaager)
        {
            manager = manaager;
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