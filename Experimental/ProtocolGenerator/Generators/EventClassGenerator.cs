using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ProtocolGenerator.Helpers;
using RakNetDotNet;

namespace ProtocolGenerator.Generators
{
    internal sealed class EventClassGenerator : IGenerator
    {
        public EventClassGenerator(Type type, int eventId, Type protocolInfoType)
        {
            this.type = type;
            this.eventId = eventId;
            this.protocolInfoType = protocolInfoType;
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
            o.BeginBlock("internal partial class {0} : IEvent {{", type.Name);
            WriteCtorWithId(o);
            WriteCtorWithStream(o);
            WriteAccessors(o);
            WriteGetStream(o);
            WriteId(o);
            WriteSourceOid(o);
            WriteTargetOid(o);
            WriteSender(o);
            WriteProtocolInfo(o);
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
            WriteStreamReadStatement(o, "out", "sourceOId");
            WriteStreamReadStatement(o, "out", "targetOId");

            foreach (FieldInfo field in GetSerializableFields())
            {
                WriteSerializeFieldStatementRecursive(o, false, field.FieldType, field.Name);
            }
            o.EndBlock("}");
        }

        [Obsolete]
        private void WriteSetData(ICodeWriter o)
        {
            StringBuilder arg = new StringBuilder();
            FieldInfo[] fields = GetSerializableFields();
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

        private void WriteAccessors(ICodeWriter o)
        {
            FieldInfo[] fields = GetSerializableFields();
            foreach (FieldInfo info in fields)
            {
                ClassGeneratorHelper.WriteAccessor(o, info.FieldType.ToString(), info.Name);
            }
        }

        private static void WriteStreamReadStatement(ICodeWriter o, string modifier, string variableName)
        {
            o.WriteLine("if (!source.Read({0} {1})) {{ throw new DeserializationException(); }}", modifier, variableName);
        }

        // TODO: ProtocolGenerator can't handle null reference.
        private void WriteGetStream(ICodeWriter o)
        {
            o.BeginBlock("public BitStream Stream {");
            o.BeginBlock("get {");
            o.WriteLine("BitStream eventStream = new BitStream();");
            WriteStreamWriteStatement(o, "id");
            WriteStreamWriteStatement(o, "sourceOId");
            WriteStreamWriteStatement(o, "targetOId");

            foreach (FieldInfo field in GetSerializableFields())
            {
                WriteSerializeFieldStatementRecursive(o, true, field.FieldType, field.Name);
            }
            o.WriteLine("return eventStream;");
            o.EndBlock("}");
            o.EndBlock("}");
        }

        private static void WriteStreamWriteStatement(ICodeWriter o, string variableName)
        {
            o.WriteLine("eventStream.Write({0});", variableName);
        }

        private static void WriteSerializeNonCollectionStatement(ICodeWriter o, bool writeToBitstream, Type variableType, string variableName)
        {
            if (BitstreamSerializationHelper.DoesSupportPrimitiveType(variableType))
            {
                WriteStreamWriteOrReadStatement(o, writeToBitstream, "out", variableName);
            }
            else if (variableType.Equals(typeof (NetworkID)) || variableType.Equals(typeof (SystemAddress)))
            {
                WriteStreamWriteOrReadStatement(o, writeToBitstream, "", variableName);
            }
            else if (variableType.IsEnum)
            {
                // TODO
            }
            else
            {
                throw new ApplicationException("This type " + variableType + " doesn't support.");
            }
        }

        private static void WriteSerializeFieldStatementRecursive(ICodeWriter o, bool writeToBitstream, Type variableType, string variableName)
        {
            if (variableType.IsArray)
            {
                Type elemType = variableType.GetElementType();
                if (writeToBitstream)
                {
                    WriteStreamWriteStatement(o, variableName + ".Length");
                    o.BeginBlock("for (int i = 0; i < {0}.Length; i++) {{", variableName);
                    WriteSerializeFieldStatementRecursive(o, writeToBitstream, elemType, variableName + "[i]");
                    o.EndBlock("}");
                }
                else
                {
                    string lengthVariableName = "_" + variableName + "Length";
                    o.WriteLine("int {0};", lengthVariableName);
                    WriteStreamReadStatement(o, "out", lengthVariableName);
                    o.WriteLine("{0} = new {1}[{2}];", variableName, elemType.ToString(), lengthVariableName);
                    o.BeginBlock("for (int i = 0; i < {0}; i++) {{", lengthVariableName);
                    WriteSerializeFieldStatementRecursive(o, writeToBitstream, elemType, variableName + "[i]");
                    o.EndBlock("}");
                }
            }
            else
            {
                WriteSerializeNonCollectionStatement(o, writeToBitstream, variableType, variableName);
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
            ClassGeneratorHelper.WriteProperty(o, "int", "id", null, null, "protected");
        }

        private static void WriteSourceOid(ICodeWriter o)
        {
            ClassGeneratorHelper.WriteProperty(o, "int", "sourceOId", null, null, null);
        }

        private static void WriteTargetOid(ICodeWriter o)
        {
            ClassGeneratorHelper.WriteProperty(o, "int", "targetOId", null, null, null);
        }

        private static void WriteSender(ICodeWriter o)
        {
            ClassGeneratorHelper.WriteProperty(o, "SystemAddress", "sender", "RakNetBindings.UNASSIGNED_SYSTEM_ADDRESS", null, null);
        }

        private void WriteProtocolInfo(ICodeWriter o)
        {
            ClassGeneratorHelper.WriteGetAccessor(o, "IProtocolInfo", "ProtocolInfo", protocolInfoType.FullName+".Instance");
        }

        private FieldInfo[] GetSerializableFields()
        {
            FieldInfo[] allFields = GetAllFields();
            List<FieldInfo> serializableFields = new List<FieldInfo>(allFields);
            RemoveNotSerializedFields(serializableFields);
            return serializableFields.ToArray();
        }

        private static void RemoveNotSerializedFields(List<FieldInfo> serializableFields)
        {
            serializableFields.RemoveAll(delegate(FieldInfo fi) { return fi.IsNotSerialized; });
        }

        private FieldInfo[] GetAllFields()
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private readonly Type type;
        private readonly int eventId;
        private readonly Type protocolInfoType;
    }
}