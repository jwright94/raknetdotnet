using Castle.Core;
using Castle.Core.Logging;

namespace EventSystem
{
    [Transient]
    internal class ClientDOManager : DOManager, IClientDOManager
    {
        private IClientCommunicator communicator;

        public ClientDOManager(ILogger logger)
            : base(logger)
        {
        }

        public IClientCommunicator ClientCommunicator
        {
            get { return communicator; }
            set { communicator = value; }
        }

        public void StoreObject(IDObject dObject)
        {
            if (!dObjects.Values.Contains(dObject))
            {
                dObjects.Add(dObject.OId, dObject);
                return;
            }

            logger.Error("Duplicate DObject found.", dObject.OId);
        }

        public override void SendEvent(IEvent e)
        {
            ClientCommunicator.SendEvent(e);
        }

        public override void PostEvent(IEvent e)
        {
            GetObject(e.TargetOId).HandleEvent(e);
        }
    }
}