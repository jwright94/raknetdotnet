using System;
using Castle.Core;
using Castle.Core.Logging;

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
            GetObject(e.TargetOId).HandleEvent(e);
        }

        public override void SendEvent(IEvent e)
        {
            throw new System.NotSupportedException();
        }
    }
}