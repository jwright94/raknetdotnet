using System;
using RakNetDotNet;

namespace EventSystem
{
    internal interface IEventFactory
    {
        IEvent RecreateSimpleEvent(BitStream source);
        [Obsolete]
        IComplecatedEvent RecreateEvent(BitStream source);
    }
}