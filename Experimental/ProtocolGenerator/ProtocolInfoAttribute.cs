using System;

namespace ProtocolGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProtocolInfoAttribute : Attribute
    {
        public ProtocolInfoAttribute(string protocolName, int majorVersion)
        {
            this.protocolName = protocolName;
            this.majorVersion = majorVersion;
        }       

        public int MajorVersion
        {
            get { return majorVersion; }
        }

        private readonly int majorVersion;

        public string ProtocolName
        {
            get { return protocolName; }
        }

        private readonly string protocolName;
    }
}