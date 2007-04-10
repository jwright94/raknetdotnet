using RakNetDotNet;

namespace EventSystem
{
    public interface IEvent
    {
        BitStream Stream { get; }
        int Id { get; }
        int SourceOid { get; set; }
        int TargetOid { get; set; }
        SystemAddress Sender { get; set; }
        IProtocolInfo ProtocolInfo { get; }
    }
}