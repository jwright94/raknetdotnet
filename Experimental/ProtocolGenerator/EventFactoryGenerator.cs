using System;
using System.Collections.Generic;

namespace ProtocolGenerator
{
    internal sealed class EventFactoryGenerator : IGenerator
    {
        public EventFactoryGenerator(string factoryName, IList<EventInfo> eventInfos)
        {
            this.factoryName = factoryName;
            this.eventInfos = eventInfos;
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
            o.WriteLine("[Transient]");
            o.BeginBlock("sealed class {0} : IEventFactory {{", factoryName);
            WriteRecreateEvent(o);
            o.WriteLine("public IComplecatedEvent RecreateEvent(BitStream source) { return null; }");
            o.EndBlock("}");
        }

        private void WriteRecreateEvent(ICodeWriter o)
        {
            o.BeginBlock("public IEvent RecreateSimpleEvent(BitStream source) {");
            o.WriteLine("Debug.Assert(source != null);");
            o.WriteLine("IEvent _event;");
            o.WriteLine("int id;");
            o.WriteLine("if(!source.Read(out id)) throw new NetworkException(\"Deserialization is failed.\");");
            o.WriteLine("source.ResetReadPointer();");
            o.BeginBlock("switch (id) {");
            foreach (EventInfo ei in eventInfos)
            {
                o.BeginBlock("case {0}:", ei.EventId);
                o.WriteLine("_event = new {0}(source);", ei.Type.Name);
                o.WriteLine("break;");
                o.EndBlock("");
            }
            o.BeginBlock("default:");
            o.WriteLine(
                "throw new NetworkException(string.Format(\"Event id {{0}} not recognized by {0}.RecreateEvent()!\", id));",
                factoryName);
            o.EndBlock("");
            o.EndBlock("}");
            o.WriteLine("return _event;");
            o.EndBlock("}");
        }

        private string factoryName;
        private IList<EventInfo> eventInfos;
    }
}