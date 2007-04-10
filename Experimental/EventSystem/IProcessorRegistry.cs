using RakNetDotNet;

namespace EventSystem
{
    internal interface IProcessorRegistry
    {
        void Add(RakPeerInterface recipient, IProtocolProcessor processor);
        void Remove(RakPeerInterface recipient, IProtocolProcessor processor);
        IProtocolProcessor GetProcessor(RakPeerInterface recipient, string protocolName);
    }
}