using System;
using System.Collections;
using Castle.Core;
using Castle.Core.Logging;
using RakNetDotNet;

namespace EventSystem
{
    [Transient]
    internal sealed class ServerCommunicator : IServerCommunicator
    {
        private readonly IDictionary props;
        private readonly ILogger logger;
        private readonly CommunicatorModule module;
        public ServerCommunicator(IDictionary props, IProcessorRegistry registry, ILogger logger)
        {
            this.props = props;
            this.logger = logger;
            module = new CommunicatorModule(registry, logger);
        }

        public IProtocolProcessorsLocator ProcessorsLocator
        {
            get { return module.ProcessorsLocator; }
            set { module.ProcessorsLocator = value; }
        }

        public void Startup()
        {
            ushort allowedPlayers = (ushort)props["allowedplayers"];
            int threadSleepTimer = (int)props["threadsleeptimer"];
            ushort port = (ushort)props["port"];
            module.Startup(allowedPlayers, threadSleepTimer, port, GetPlugins());
        }

        private static PluginInterface[] GetPlugins()
        {
            // TODO - I don't implement a feature of distributed server now.
            //ConnectionGraph connectionGraphPlugin = RakNetworkFactory.GetConnectionGraph(); // TODO - Do Destroy?
            //FullyConnectedMesh fullyConnectedMeshPlugin = new FullyConnectedMesh(); // TODO - Do Dispose?

            //fullyConnectedMeshPlugin.Startup(string.Empty);

            //return new PluginInterface[] {connectionGraphPlugin, fullyConnectedMeshPlugin};
            return new PluginInterface[]{};
        }

        public void Update()
        {
            module.Update();
        }

        public void Shutdown()
        {
            module.Shutdown();
        }

        public void Broadcast(string processorName, IEvent e)
        {
            PacketPriority priority = PacketPriority.HIGH_PRIORITY;
            PacketReliability reliability = PacketReliability.RELIABLE_ORDERED;
            byte orderingChannel = 0;
            uint shiftTimestamp = 0;

            logger.Debug("sending an event: [{0}]", e.ToString());

            bool result = module.RakPeerInterface.RPC(
                processorName,
                e.Stream, priority, reliability, orderingChannel,
                RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, shiftTimestamp,
                RakNetBindings.UNASSIGNED_NETWORK_ID, null);

            if (!result)
                logger.Debug("could not send data to clients!");
            else
                logger.Debug("send data to clients...");
        }

        public void SendEvent(string processorName, IEvent e)
        {
            throw new NotImplementedException();
        }
    }
}