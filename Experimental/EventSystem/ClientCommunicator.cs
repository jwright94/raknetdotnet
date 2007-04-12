using System;
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

        public void SendEvent(IEvent e)
        {
            module.SendEvent(RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, e);
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

        public void InjectProcessorLocator(IProtocolProcessorLocator locator)
        {
            module.InjectProcessorLocator(locator);
        }

        public void RegisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            module.RegisterRakNetEventHandler(messageId, handler);
        }

        public void UnregisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler)
        {
            module.UnregisterRakNetEventHandler(messageId, handler);
        }
    }
}