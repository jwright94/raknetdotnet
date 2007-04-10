using System;
using RakNetDotNet;

namespace EventSystem
{
    public class ServerDOManager : IServerDoManager
    {

        public ServerDOManager()
        {
        }

        #region IServerDoManager Members

        public IDObject GetObject(int oId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RegisterObject(IDObject dObject)
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