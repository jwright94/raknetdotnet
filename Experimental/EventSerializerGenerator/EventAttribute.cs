using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventAttribute : Attribute
    {
        public bool IsBroadcast = false;
        public bool IsTwoWay = false;
        public bool RunOnServer = false;
        public bool PerformBeforeConnectOnClient = false;
    }
}
