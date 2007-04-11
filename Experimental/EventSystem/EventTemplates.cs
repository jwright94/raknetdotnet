using ProtocolGenerator;

namespace Events
{
    [ProtocolInfoAttribute("SampleProtocol", 1)]
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

        public int NewOid
        {
            get { return newOid; }
            set { newOid = value; }
        }
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
        public string Color
        {
            get { return color; }
            set { color = value; }
        }
    }

    [SiteOfHandling("Client")]
    [SiteOfHandling("ClientActor")]
    public partial class ChangeColor
    {
        private string color;
        public string Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}