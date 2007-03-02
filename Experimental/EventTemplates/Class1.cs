using System;
using System.Collections.Generic;
using System.Text;

namespace EventTemplates
{
    using RakNetDotNet;

    public partial class RegisterEvent
    {
        string name;
        SystemAddress[] systemAddresses;
        byte serviceId;
    }
}
