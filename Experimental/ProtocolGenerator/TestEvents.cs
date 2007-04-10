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

    [ProtocolInfoAttribute("TestProtocol")]
    public partial class ProtocolInfo
    {
    }
}