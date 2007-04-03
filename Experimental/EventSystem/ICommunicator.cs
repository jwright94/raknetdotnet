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
        EventHandlersType GetEventHandlers<EventHandlersType>(string processorName);
    }
}