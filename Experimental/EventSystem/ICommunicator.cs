using RakNetDotNet;

namespace EventSystem
{
    internal interface ICommunicator
    {
        void Startup();
        void Update();
        void Shutdown();

        IProtocolProcessorLocator ProcessorLocator { get; set; }

        void RegisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler);
        void UnregisterRakNetEventHandler(RakNetMessageId messageId, RakNetEventHandler handler);
    }

    internal interface IServerCommunicator : ICommunicator
    {
        void Broadcast(IEvent e);
        void SendEvent(SystemAddress targetAddress, IEvent e);
    }

    /// <summary>
    /// Communication interface between client and frontend server.
    /// </summary>
    internal interface IClientCommunicator : ICommunicator
    {
        void Connect();
        void SendEvent(IEvent e);
    }
}