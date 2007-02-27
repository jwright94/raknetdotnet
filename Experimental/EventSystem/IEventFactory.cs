using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    interface IEventFactory
    {
        IEvent RecreateEvent(BitStream source);
    }
}
