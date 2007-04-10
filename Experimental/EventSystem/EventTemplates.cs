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

    [ProtocolInfoAttribute("SampleProtocol2")]
    public partial class ProtocolInfo
    {
    }
}