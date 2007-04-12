using RakNetDotNet;

namespace EventSystem
{
    internal interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        int SourceOId { get; set; }
        int TargetOId { get; set; }
        SystemAddress Sender { get; set; }
        IProtocolInfo ProtocolInfo { get; }
    }
}