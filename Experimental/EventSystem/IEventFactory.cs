using RakNetDotNet;

namespace EventSystem
{
    internal interface IEventFactory
    {
        IEvent RecreateSimpleEvent(BitStream source);
    }
}