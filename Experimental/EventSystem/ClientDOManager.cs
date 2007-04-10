using System;
using RakNetDotNet;
using Castle.Core.Logging;
using Castle.Core;

namespace EventSystem
{
    [Transient]
    public class ClientDOManager : DOManager, IClientDOManager
    {
        public ClientDOManager(ILogger logger)
            : base(logger)
        {
        }

        public void StoreObject(IDObject dObject)
        {
            if (!dObjects.ContainsValue(dObject))
            {
                dObjects.Add(dObject.OId, dObject);
                return;
            }
            logger.Error("Duplicate DObject found.", dObject.OId);
        }
    }
}