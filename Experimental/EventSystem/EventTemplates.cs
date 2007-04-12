using ProtocolGenerator;

namespace Events
{
    [ProtocolInfo("SampleProtocol", 1)]
    internal partial class ProtocolInfo
    {
    }

    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("Client")]
    internal partial class ConnectionTest
    {
    }

    [SiteOfHandling("FrontEndServer")]
    internal partial class LogOnEvent
    {
    }

    [SiteOfHandling("Client")]
    internal partial class LogOnACK
    {
        private int newOId;
    }

    [SiteOfHandling("Client")]
    internal partial class TestDOEvent
    {
    }

    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("ServerActor")]
    internal partial class ChangeColorRequest
    {
        private string color;
    }

    [SiteOfHandling("Client")]
    [SiteOfHandling("ClientActor")]
    internal partial class ChangeColor
    {
        private string color;
    }
}