using System.Collections;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    [Transient]
    internal sealed class ClientCommunicator : IClientCommunicator
    {
        private readonly IDictionary props;
        private readonly ILogger logger;
        private readonly CommunicatorModule module;

        public ClientCommunicator(IDictionary props, IProcessorRegistry registry, ILogger logger)
        {
            this.props = props;
            this.logger = logger;
            module = new CommunicatorModule(registry, logger);
        }

        public void Connect()
        {
            string serverAddr = (string)props["fsaddr"];
            ushort serverPort = (ushort)props["fsport"];
            module.RakPeerInterface.Connect(serverAddr, serverPort, string.Empty, 0);
        }

        // TODO - Refactor this.
        public void SendEvent(IEvent e)
        {
            PacketPriority priority = PacketPriority.HIGH_PRIORITY;
            PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
            byte orderingChannel = 0;
            uint shiftTimestamp = 0;

            logger.Debug("sending an event: [{0}]", e.ToString());

            bool result = module.RakPeerInterface.RPC(
                e.ProtocolInfo.Name,
                e.Stream, priority, reliability, orderingChannel,
                RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, shiftTimestamp,
                RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            if (!result)
                logger.Debug("could not send data to the server!");
            else
                logger.Debug("send data to the server...");
        }

        public void Startup()
        {
            int threadSleepTimer = (int)props["threadsleeptimer"];
            module.Startup(1, threadSleepTimer, 0);
        }

        public void Update()
        {
            module.Update();
        }

        public void Shutdown()
        {
            module.Shutdown();
        }

        public IProtocolProcessorLocator ProcessorLocator
        {
            get { return module.ProcessorLocator; }
            set { module.ProcessorLocator = value; }
        }

        #region ICommunicator Members

        public void RegisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            module.RegisterRakNetEventHandler(messageId, handler);
        }

        public void UnregisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            module.UnregisterRakNetEventHandler(messageId, handler);
        }

        #endregion
    }
}