using System;
using RakNetDotNet;

namespace EventSystem
{
    [Obsolete]
    internal interface IComplecatedEvent
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

    internal interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        SystemAddress Sender { get; set; }
    }
}