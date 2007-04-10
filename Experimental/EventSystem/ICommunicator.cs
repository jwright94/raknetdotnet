using System;
using RakNetDotNet;

namespace EventSystem
{
    interface ICommunicator
    {
        void Startup();
        void Update();
        void Shutdown();

        IProtocolProcessorLocator ProcessorLocator
        {
            get;
            set;
        }
    }

    interface IServerCommunicator : ICommunicator
    {
        void Broadcast(IEvent e);
        void SendEvent(IEvent e);
    }

    /// <summary>
    /// Communication interface between client and frontend server.
    /// </summary>
    interface IClientCommunicator : ICommunicator
    {
        void Connect();
        void SendEvent(IEvent e);
    }
}