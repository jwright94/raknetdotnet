using System;

namespace ProtocolGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProtocolInfoAttribute : Attribute
    {
        public ProtocolInfoAttribute(string protocolName)
        {
            this.protocolName = protocolName;
        }

        private string protocolName;

        public string ProtocolName
        {
            get { return protocolName; }
        }
    }
}