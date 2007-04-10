using System;

namespace ProtocolGenerator
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
            WriteSingleton(o, className);
            WriteName(o, protocolName);
            WriteVersionProperty(o, "MajorVersion", attr.MajorVersion);
            WriteVersionProperty(o, "MinorVersion", minorVersion);
            o.EndBlock("}");
        }

        private static void WriteVersionProperty(ICodeWriter o, string propertyName, int version)
        {
            o.BeginBlock("public int {0} {{", propertyName);
            o.WriteLine("get {{ return {0}; }}", version);
            o.EndBlock("}");
        }

        private static void WriteName(ICodeWriter o, string protocolName)
        {
            o.BeginBlock("public string Name {");
            o.WriteLine("get {{ return \"{0}\"; }}", protocolName);
            o.EndBlock("}");
        }

        private static void WriteSingleton(ICodeWriter o, string className)
        {
            o.WriteLine("private static {0} instance = new {0}();", className);
            o.BeginBlock("public static {0} Instance {{", className);
            o.WriteLine("get { return instance; }");
            o.EndBlock("}");
            o.WriteLine("private {0}() {{ }}", className);
        }
    }
}