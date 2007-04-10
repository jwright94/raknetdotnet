using System;
using RakNetDotNet;
using Castle.Core.Logging;

namespace EventSystem
{
    public class ClientDOManager : DOManager, IClientDoManager
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