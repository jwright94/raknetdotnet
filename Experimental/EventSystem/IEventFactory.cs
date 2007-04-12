using RakNetDotNet;

namespace EventSystem
{
    internal interface IEventFactory
    {
        // TODO - Rename RecreateSimpleEvent to RecreateEvent
        IEvent RecreateSimpleEvent(BitStream source);
    }
}