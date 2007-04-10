using System;
using RakNetDotNet;

namespace EventSystem
{
    public class DObject: IDObject
    {
        private int oId;

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
            throw new Exception("The method or operation is not implemented.");
        }

        public void PostEvent(IEvent e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}