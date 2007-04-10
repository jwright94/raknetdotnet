using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TestEvents;
using TestEvents2;

namespace ProtocolGenerator
{
    internal sealed class ProtocolInfoGenerator : IGenerator
    {
        private Type t;
        private Attribute attribute;

        public ProtocolInfoGenerator(Type t, Attribute attribute)
        {
            this.t = t;
            this.attribute = attribute;
        }

        #region IGenerator Members

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
            string protocolName = (attribute != null) ? ((ProtocolInfoAttribute)attribute).ProtocolName : "";
            o.BeginBlock("public partial class {0} : IProtocolInfo {{", className);
            o.WriteLine("private static {0} instance = new {0}();", className);
            o.BeginBlock("public static {0} Instance {{", className);
            o.WriteLine("get { return instance; }");
            o.EndBlock("}");
            o.WriteLine("private {0}() {{ }}", className);
            o.BeginBlock("public string Name {");
            o.WriteLine("get {{ return \"{0}\"; }}", protocolName);
            o.EndBlock("}");
            o.EndBlock("}");
        }

        #endregion
    }
}