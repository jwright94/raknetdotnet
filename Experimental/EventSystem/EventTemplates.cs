using ProtocolGenerator;

namespace Events
{
    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("Client")]
    public partial class ConnectionTest
    {
    }
}

namespace AnotherEvents
{
    [SiteOfHandling("Server")]
    public partial class SampleEvent
    {
        // TODO: ProtocolGenerator can't handle null reference.
        private string name;
    }

    [SiteOfHandling("Client")]
    public partial class OtherEvent
    {
        private int[] intArray;
    }
}