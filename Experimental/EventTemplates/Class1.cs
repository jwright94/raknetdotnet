using System;
using System.Collections.Generic;
using System.Text;

namespace EventTemplates
{
    using RakNetDotNet;

    public class RegisterEvent
    {
        public string name;
        public SystemAddress[] systemAddresses;
        public byte serviceId;
    }
}
