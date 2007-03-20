using System;
using System.Collections.Generic;
using System.Text;

namespace SampleEvents
{
    using RakNetDotNet;
    using EventSerializerGenerator;

    [Event(
        IsBroadcast=true, 
        IsTwoWay=false, 
        PerformBeforeConnectOnClient=false, 
        RunOnServer=true)]
    public partial class RegisterEvent
    {
        string name;
        SystemAddress[] systemAddresses;
        byte serviceId;
    }
}
namespace AnotherSampleEvents
{
    public partial class OtherEvent
    {
    }
}
