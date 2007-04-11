using ProtocolGenerator;
using RakNetDotNet;

namespace TestEvents
{
    [ProtocolInfo("TestProtocol", 1)]
    public partial class ProtocolInfo
    {
    }

    [SiteOfHandling("Server")]
    public partial class SimpleEvent
    {
        private string name;
        private int age;
        private SystemAddress[] systemAddresses;
    }
}