using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RakNetDotNet;

namespace EventSerializerGenerator
{
    internal sealed class ClassGenerator : IGenerator
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

        private void WriteCtorWithId(ICodeWriter o)
        {
            o.BeginBlock("public {0}() {{", type.Name);
            o.WriteLine("id = {0};", eventId);
            o.EndBlock("}");
        }

        private void WriteCtorWithStream(ICodeWriter o)
        {
            o.BeginBlock("public {0}(BitStream source) {{", type.Name);
            WriteStreamReadStatement(o, "out", "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteSerializeFieldStatement(o, false, field);
            }
            o.EndBlock("}");
        }

        private void WriteSetData(ICodeWriter o)
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

        private static void WriteStreamReadStatement(ICodeWriter o, string modifier, string variableName)
        {
            o.WriteLine("if (!source.Read({0} {1})) {{ throw new NetworkException(\"Deserialization is failed.\"); }}",
                        modifier, variableName);
        }

        private void WriteGetStream(ICodeWriter o)
        {
            o.BeginBlock("public BitStream Stream {");
            o.BeginBlock("get {");
            o.WriteLine("BitStream eventStream = new BitStream();");
            WriteStreamWriteStatement(o, "id");
            foreach (FieldInfo field in GetFields())
            {
                WriteSerializeFieldStatement(o, true, field);
            }
            o.WriteLine("return eventStream;");
            o.EndBlock("}");
            o.EndBlock("}");
        }

        private static void WriteStreamWriteStatement(ICodeWriter o, string variableName)
        {
            o.WriteLine("eventStream.Write({0});", variableName);
        }

        private static void WriteSerializeFieldStatement(ICodeWriter o, bool writeToBitstream, FieldInfo fi)
        {
            Type fieldType = fi.FieldType;
            string fieldName = fi.Name;
            if (BitstreamSerializationHelper.DoesSupportPrimitiveType(fieldType))
            {
                WriteStreamWriteOrReadStatement(o, writeToBitstream, "out", fieldName);
            }
            else if (fieldType.Equals(typeof (NetworkID)) || fieldType.Equals(typeof (SystemAddress)))
            {
                WriteStreamWriteOrReadStatement(o, writeToBitstream, "", fieldName);
            }
            else if (fieldType.IsArray)
            {
                WriteSerializeArrayStatement(o, writeToBitstream, fieldType, fieldName);
            }
            else if (fieldType.IsEnum)
            {
                // TODO
            }
            else
            {
                throw new ApplicationException("This type " + fieldType + " doesn't support.");
            }
        }

        private static void WriteSerializeArrayStatement(ICodeWriter o, bool writeToBitstream, Type fieldType, string fieldName)
        {
            Type elemType = fieldType.GetElementType();
            if (BitstreamSerializationHelper.DoesSupportPrimitiveType(elemType))
            {
                if (writeToBitstream)
                {
                    WriteStreamWriteStatement(o, fieldName + ".Length");
                    o.BeginBlock("for (int i = 0; i < {0}.Length; i++) {{", fieldName);
                    WriteStreamWriteStatement(o, fieldName + "[i]");
                    o.EndBlock("}");
                }
                else
                {
                    string lengthVariableName = "_" + fieldName + "Length";
                    o.WriteLine("int {0};", lengthVariableName);
                    WriteStreamReadStatement(o, "out", lengthVariableName);
                    o.WriteLine("{0} = new {1}[{2}];", fieldName, elemType.ToString(), lengthVariableName);
                    o.BeginBlock("for (int i = 0; i < {0}; i++) {{", lengthVariableName);
                    WriteStreamReadStatement(o, "out", fieldName + "[i]");
                    o.EndBlock("}");
                }
            }
            else
            {
                throw new ApplicationException("This type " + elemType + " doesn't support.");
            }
        }

        private static void WriteStreamWriteOrReadStatement(ICodeWriter o, bool writeToBitstream, string modifier,
                                                     string variableName)
        {
            if (writeToBitstream)
                WriteStreamWriteStatement(o, variableName);
            else
                WriteStreamReadStatement(o, modifier, variableName);
        }

        private static void WriteId(ICodeWriter o)
        {
            o.BeginBlock("public int Id {");
            o.WriteLine("get { return id; }");
            o.WriteLine("protected set { id = value; }");
            o.EndBlock("}");
            o.WriteLine("int id;");
        }

        private static void WriteOriginPlayer(ICodeWriter o)
        {
            o.BeginBlock("public SystemAddress Sender {");
            o.WriteLine("get { return sender; }");
            o.WriteLine("set { sender = value; }");
            o.EndBlock("}");
            o.WriteLine("SystemAddress sender = RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS;");
        }

        private FieldInfo[] GetFields()
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private Type type;
        private int eventId;
    }

    internal static class BitstreamSerializationHelper
    {
        static BitstreamSerializationHelper()
        {
            supportingPrimitives = new List<Type>();
            supportingPrimitives.Add(typeof (bool));
            supportingPrimitives.Add(typeof (byte));
            supportingPrimitives.Add(typeof (double));
            supportingPrimitives.Add(typeof (float));
            supportingPrimitives.Add(typeof (int));
            supportingPrimitives.Add(typeof (sbyte));
            supportingPrimitives.Add(typeof (short));
            supportingPrimitives.Add(typeof (string));
            supportingPrimitives.Add(typeof (uint));
            supportingPrimitives.Add(typeof (ushort));
        }

        public static bool DoesSupportPrimitiveType(Type t)
        {
            return supportingPrimitives.Contains(t);
        }

        private static IList<Type> supportingPrimitives;
    }
}