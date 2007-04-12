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
            return new PluginInterface[] {};
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

        public void Broadcast(IEvent e)
        {
            module.SendEvent(RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS, true, e);
        }

        public void SendEvent(SystemAddress targetAddress, IEvent e)
        {
            module.SendEvent(targetAddress, false, e);
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