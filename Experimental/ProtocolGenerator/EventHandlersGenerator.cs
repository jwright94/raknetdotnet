using System;
using System.Collections.Generic;

namespace ProtocolGenerator
{
    internal sealed class EventHandlersGenerator : IGenerator
    {
        public EventHandlersGenerator(string handlersName, IList<EventInfo> eventInfos)
        {
            this.handlersName = handlersName;
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
            o.WriteLine("[Singleton]");
            o.BeginBlock("sealed class {0} : IEventHandlers {{", handlersName);
            WriteCallHandler(o);
            WriteEvents(o);
            o.EndBlock("}");
        }

        private void WriteCallHandler(ICodeWriter o)
        {
            o.BeginBlock("public void CallHandler(IEvent e) {");
            o.BeginBlock("switch (e.Id) {");
            foreach (EventInfo ei in eventInfos)
            {
                o.BeginBlock("case {0}:", ei.EventId);
                string handlerName = GetHandlerName(ei.Type.Name);
                o.BeginBlock("if ({0} != null)", handlerName);
                o.WriteLine("{0}(({1})e);", handlerName, ei.Type.Name);
                o.EndBlock("break;");
                o.EndBlock("");
            }
            o.BeginBlock("default:");
            o.WriteLine(
                "throw new NetworkException(string.Format(\"Event id {{0}} not recognized by {0}.CallHandler()!\", e.Id));",
                handlersName);
            o.EndBlock("");
            o.EndBlock("}");
            o.EndBlock("}");
        }

        private void WriteEvents(ICodeWriter o)
        {
            foreach (EventInfo ei in eventInfos)
            {
                o.WriteLine("public event EventHandler<{0}> {1};", ei.Type.Name, GetHandlerName(ei.Type.Name));
            }
        }

        private static string GetHandlerName(string typeName)
        {
            return NamingHelper.GetPrefix(typeName, "Event");
        }

        private string handlersName;
        private IList<EventInfo> eventInfos;
    }
}