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
}

namespace TestEvents2
{
    [SiteOfHandling("Client")]
    public partial class SimpleEvent2
    {
        private string name;
    }
}