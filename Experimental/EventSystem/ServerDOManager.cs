using System;
using System.Collections.Generic;
using RakNetDotNet;
using Castle.Core.Logging;
using Castle.Core;


namespace EventSystem
{
    [Transient]
    public class ServerDOManager : DOManager, IServerDOManager
    {
        private int nextId = 0;

        public ServerDOManager(ILogger logger)
            : base(logger)
        {
        }

        #region IServerDoManager Members

        public int RegisterObject(IDObject dObject)
        {
            if (!dObjects.ContainsValue(dObject))
            {
                dObject.OId = nextId;
                dObjects.Add(nextId++, dObject);
                return dObject.OId;
            }
            logger.Error("Duplicate DObject found.", dObject.OId);
            return -1;
        }

        public override void PostEvent(IEvent e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}