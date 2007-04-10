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
}