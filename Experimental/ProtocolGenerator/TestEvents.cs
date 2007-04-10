using ProtocolGenerator;
using RakNetDotNet;

namespace TestEvents
{
    [SiteOfHandling("Server")]
    public partial class SimpleEvent
    {
        private string name;
        private int age;
        private SystemAddress[] systemAddresses;
    }

    [ProtocolInfoAttribute("SampleProtocol")]
    public partial class ProtocolInfo
    {
    }
}

namespace TestEvents2
{
    [SiteOfHandling("Client")]
    public partial class SimpleEvent2
    {
        private string name;
    }

    [ProtocolInfoAttribute("SampleProtocol2")]
    public partial class ProtocolInfo
    {
    }
}