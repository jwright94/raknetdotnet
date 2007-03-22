using System;
using System.Collections.Generic;
using System.Text;

namespace EventSystem
{
    using RakNetDotNet;

    interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        void Perform();
        bool IsBroadcast { get; }
        bool IsTwoWay { get; }
        SystemAddress OriginPlayer { get; set; }
        bool RunOnServer { get; }
        bool PerformBeforeConnectOnClient { get; }
    }

    interface ISimpleEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        SystemAddress OriginPlayer { get; set; }
    }
}
