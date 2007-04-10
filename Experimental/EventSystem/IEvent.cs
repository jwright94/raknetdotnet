using System;
using RakNetDotNet;

namespace EventSystem
{
    public interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        int SourceOid { get; }
        int TargetOid { get;}
        SystemAddress Sender { get; set; }
        IProtocolInfo ProtocolInfo { get; }
    }
}