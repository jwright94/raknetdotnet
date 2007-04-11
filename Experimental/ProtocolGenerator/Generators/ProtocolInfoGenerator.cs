using System;
using ProtocolGenerator.Helpers;

namespace ProtocolGenerator.Generators
{
    internal sealed class ProtocolInfoGenerator : IGenerator
    {
        private readonly Type t;
        private readonly ProtocolInfoAttribute attr;
        private readonly int minorVersion;

        public ProtocolInfoGenerator(Type t, ProtocolInfoAttribute attr, int minorVersion)
        {
            this.t = t;
            this.attr = attr;
            this.minorVersion = minorVersion;
        }

        public void AddChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveChildGenerator(IGenerator generator)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(ICodeWriter o)
        {
            string className = t.Name;
            string protocolName = attr.ProtocolName;
            o.BeginBlock("public partial class {0} : IProtocolInfo {{", className);
            ClassGeneratorHelper.WriteSingleton(o, className);
            WriteName(o, protocolName);
            WriteVersionProperty(o, "MajorVersion", attr.MajorVersion);
            WriteVersionProperty(o, "MinorVersion", minorVersion);
            o.EndBlock("}");
        }

        private static void WriteVersionProperty(ICodeWriter o, string propertyName, int version)
        {
            ClassGeneratorHelper.WriteGetProperty(o, "int", propertyName, version.ToString());
        }

        private static void WriteName(ICodeWriter o, string protocolName)
        {
            ClassGeneratorHelper.WriteGetProperty(o, "string", "Name", '"' + protocolName + '"');
        }
    }
}