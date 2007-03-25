using ProtocolGenerator;
using RakNetDotNet;

namespace SampleEvents
{
    [SiteOfHandling("Server")]
    public partial class RegisterEvent
    {
        private string name;
        SystemAddress[] systemAddresses;
        private byte serviceId;
    }
}

namespace AnotherSampleEvents
{
    [SiteOfHandling("Server")]
    [SiteOfHandling("Client")]
    public partial class OtherEvent
    {
    }
}