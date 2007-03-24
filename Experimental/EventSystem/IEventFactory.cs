using RakNetDotNet;

namespace EventSystem
{
    internal interface IEventFactory
    {
        ISimpleEvent RecreateSimpleEvent(BitStream source);
        IEvent RecreateEvent(BitStream source);
    }
}