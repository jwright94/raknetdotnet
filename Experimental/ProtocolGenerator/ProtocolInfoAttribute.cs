using System;

namespace ProtocolGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProtocolInfoAttribute : Attribute
    {
        public ProtocolInfoAttribute(string protocolName, uint majorVersion)
        {
            this.protocolName = protocolName;
            this.majorVersion = majorVersion;
        }

        public uint MajorVersion
        {
            get { return majorVersion; }
        }

        private readonly uint majorVersion;

        public string ProtocolName
        {
            get { return protocolName; }
        }

        private readonly string protocolName;
    }
}