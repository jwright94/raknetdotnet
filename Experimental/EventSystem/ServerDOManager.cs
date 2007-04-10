using System;
using System.Collections.Generic;
using RakNetDotNet;
using Castle.Core.Logging;


namespace EventSystem
{
    public class ServerDOManager : DOManager, IServerDoManager
    {
        private int nextId = 0;

        public ServerDOManager(ILogger logger)
            : base(logger)
        {
        }

        #region IServerDoManager Members

        public void RegisterObject(IDObject dObject)
        {
            if (!dObjects.ContainsValue(dObject))
            {
                dObject.OId = nextId;
                dObjects.Add(nextId++, dObject);
                return;
            }
            logger.Error("Duplicate DObject found.", dObject.OId);
        }

        public override void PostEvent(IEvent e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}