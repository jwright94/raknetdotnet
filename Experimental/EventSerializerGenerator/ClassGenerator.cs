using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace EventSerializerGenerator
{
    sealed class ClassGenerator : IGenerator
    {
        public ClassGenerator(Type type, int eventId)
        {
            this.type = type;
            this.eventId = eventId;
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
            o.BeginBlock("public partial class {0} : ISimpleEvent {{", type.Name);
            WriteCtorWithId(o);
            WriteCtorWithStream(o);
            WriteSetData(o);
            WriteGetStream(o);
            WriteId(o);
            WriteOriginPlayer(o);
            o.EndBlock("}");
        }
        void WriteCtorWithId(ICodeWriter o)
        {
            o.BeginBlock("public {0}() {{", type.Name);
            o.WriteLine("id = {0};", eventId);
            o.EndBlock("}");
        }
        void WriteCtorWithStream(ICodeWriter o)
        {
            o.BeginBlock("public {0}(BitStream source) {{", type.Name);
            WriteStreamReadStatement(o, "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteStreamReadStatement(o, field.Name);
            }
            o.EndBlock("}");
        }
        void WriteSetData(ICodeWriter o)
        {
            StringBuilder arg = new StringBuilder();
            FieldInfo[] fields = GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (0 < i) arg.Append(", ");
                arg.Append(fields[i].FieldType.ToString());
                arg.Append(" ");
                arg.Append(fields[i].Name);
            }
            o.BeginBlock("public void SetData({0}) {{", arg.ToString());
            foreach (FieldInfo field in fields)
            {
                o.WriteLine("this.{0} = {0};", field.Name);
            }
            o.EndBlock("}");
        }
        void WriteStreamReadStatement(ICodeWriter o, string fieldName)
        {
            o.WriteLine("if (!source.Read(out {0})) {{ throw new NetworkException(\"Deserialization is failed.\"); }}", fieldName);
        }
        void WriteGetStream(ICodeWriter o)
        {
            o.BeginBlock("public BitStream Stream {");
            o.BeginBlock("get {");
            o.WriteLine("BitStream eventStream = new BitStream();");
            WriteStreamWriteStatement(o, "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteStreamWriteStatement(o, field.Name);
            }
            o.WriteLine("return eventStream;");
            o.EndBlock("}");
            o.EndBlock("}");
        }
        void WriteStreamWriteStatement(ICodeWriter o, string fieldName)
        {
            o.WriteLine("eventStream.Write({0});", fieldName);
        }
        void WriteId(ICodeWriter o)
        {
            o.BeginBlock("public int Id {");
            o.WriteLine("get { return id; }");
            o.WriteLine("protected set { id = value; }");
            o.EndBlock("}");
            o.WriteLine("int id;");
        }
        void WriteOriginPlayer(ICodeWriter o)
        {
            o.BeginBlock("public SystemAddress OriginPlayer {");
            o.WriteLine("get { return originPlayer; }");
            o.WriteLine("set { originPlayer = value; }");
            o.EndBlock("}");
            o.WriteLine("SystemAddress originPlayer = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;");
        }
        void WriteBehaviorFlags(ICodeWriter o)
        {
            //type.GetCustomAttributes(typeof(EventAttribute), true);
        }
        FieldInfo[] GetFields()
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        Type type;
        int eventId;
    }
}
