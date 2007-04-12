using System.Collections.Generic;
using Castle.Core.Logging;

namespace EventSystem
{
    internal abstract class DOManager : IDOManager
    {
        protected ILogger logger;
        protected IDictionary<int, IDObject> dObjects = new Dictionary<int, IDObject>();

        public DOManager(ILogger logger)
        {
            this.logger = logger;
        }

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

        public abstract void PostEvent(IEvent e);
        public abstract void SendEvent(IEvent e);
    }
}