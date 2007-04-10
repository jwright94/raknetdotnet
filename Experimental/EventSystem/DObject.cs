using System;
using RakNetDotNet;

namespace EventSystem
{
    public class DObject: IDObject
    {
        private int oId;
        public HandleEventDelegate OnGetEvent;
        public delegate void HandleEventDelegate(IEvent e);


        public DObject()
        {
        }

        #region IDObject Members

        public int OId
        {
            get { return oId; }
            set { oId = value; }
        }

        public void HandleEvent(IEvent e)
        {
        }

        public void PostEvent(IEvent e)
        {
            if (OnGetEvent != null)
            {
                OnGetEvent(e);
            }
        }

        #endregion
    }
}