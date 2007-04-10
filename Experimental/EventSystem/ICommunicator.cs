using RakNetDotNet;

namespace EventSystem
{
    internal interface ICommunicator
    {
        void Startup();
        void Update();
        void Shutdown();

        IProtocolProcessorLocator ProcessorLocator { get; set; }
    }

    internal interface IServerCommunicator : ICommunicator
    {
        void Broadcast(IEvent e);
        void SendEvent(IEvent e, SystemAddress targetAddress);
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