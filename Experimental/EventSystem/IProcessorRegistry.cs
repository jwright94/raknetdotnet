using RakNetDotNet;

namespace EventSystem
{
    interface IProcessorRegistry
    {
        void Add(RakPeerInterface recipient, IProtocolProcessor processor);
        void Remove(RakPeerInterface recipient, IProtocolProcessor processor);
        IProtocolProcessor GetProcessor(RakPeerInterface recipient, string processorName);
    }
}