using ProtocolGenerator;

namespace Events
{
    [ProtocolInfo("SampleProtocol", 1)]
    public partial class ProtocolInfo
    {
    }

    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("Client")]
    public partial class ConnectionTest
    {
    }

    [SiteOfHandling("FrontEndServer")]
    public partial class LogOnEvent
    {
    }

    [SiteOfHandling("Client")]
    public partial class LogOnACK
    {
        private int newOid;
    }

    [SiteOfHandling("Client")]
    public partial class TestDOEvent
    {
    }

    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("ServerActor")]
    public partial class ChangeColorRequest
    {
        private string color;
    }

    [SiteOfHandling("Client")]
    [SiteOfHandling("ClientActor")]
    public partial class ChangeColor
    {
        private string color;
    }
}