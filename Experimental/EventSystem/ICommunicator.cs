using System;
using RakNetDotNet;

namespace EventSystem
{
    interface ICommunicator
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