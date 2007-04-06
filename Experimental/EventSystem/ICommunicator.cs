using System;
using RakNetDotNet;

namespace EventSystem
{
    interface ICommunicator : IDisposable
    {
        void Startup();
        void Update();
        void Shutdown();
        void SendEvent(string processorName, IEvent e, SystemAddress address);

        IProtocolProcessorsLocator ProcessorsLocator
        {
            get;
            set;
        }
    }
}