using System;
using System.Collections.Generic;
using System.Text;
using RakNetDotNet;
using EventSerializerGenerator;

namespace SampleEvents
{
    [SiteOfHandling("Server")]
    public partial class RegisterEvent
    {
        string name;
        //SystemAddress[] systemAddresses;
        byte serviceId;
    }
}
namespace AnotherSampleEvents
{
    [SiteOfHandling("Server")]
    [SiteOfHandling("Client")]
    public partial class OtherEvent
    {
    }
}
