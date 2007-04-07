using System;
using RakNetDotNet;

namespace EventSystem
{
    interface ICommunicator
    {
        void Startup();
        void Update();
        void Shutdown();

        IProtocolProcessorsLocator ProcessorsLocator
        {
            get;
            set;
        }
    }

    interface IServerCommunicator : ICommunicator
    {
        void Broadcast(string processorName, IEvent e);
        void SendEvent(string processorName, IEvent e);
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