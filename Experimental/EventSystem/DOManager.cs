using System;
using RakNetDotNet;
using Castle.Core.Logging;
using System.Collections.Generic;

namespace EventSystem
{
    public abstract class DOManager : IDOManager
    {
        protected Dictionary<int, IDObject> dObjects = new Dictionary<int, IDObject>();

        protected ILogger logger;

        public DOManager(ILogger logger)
        {
            this.logger = logger;
        }

        #region IDOManager Members

        public virtual IDObject GetObject(int oId)
        {
            IDObject tempObject;
            if (dObjects.TryGetValue(oId, out tempObject))
            {
                return tempObject;
            }
            logger.Error("No DObject found with object Id. {0}", oId);
            return null;
        }

        public virtual void PostEvent(IEvent e)
        {           
            GetObject(e.TargetOid).PostEvent(e);
        }
  
        #endregion
    }
}