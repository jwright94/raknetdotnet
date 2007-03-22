using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    interface IEventFactory
    {
        ISimpleEvent RecreateSimpleEvent(BitStream source);
        IEvent RecreateEvent(BitStream source);
    }
}
