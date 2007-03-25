using ProtocolGenerator;
using RakNetDotNet;

namespace SampleEvents
{
    [SiteOfHandling("NamingServer")]
    public partial class RegisterEvent
    {
        private string name;
        SystemAddress[] systemAddresses;
        private byte serviceId;
    }
    [SiteOfHandling("NamingClient")]
    public partial class ServiceList
    {
        private string name;
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