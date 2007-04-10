using ProtocolGenerator;

namespace Events
{
    [SiteOfHandling("FrontEndServer")]
    [SiteOfHandling("Client")]
    public partial class ConnectionTest
    {
    }

    [ProtocolInfoAttribute("SampleProtocol")]
    public partial class ProtocolInfo
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
}