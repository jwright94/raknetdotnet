using System;
using System.Collections.Generic;
using System.Text;

namespace EventSerializerGenerator
{
    sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string _namespace, IEnumerable<Type> typesInNamespace)  // TODO: I forgot how to use simbol.
        {
            this._namespace = _namespace;
            IList<EventInfo> eventInfos = new List<EventInfo>();
            foreach (Type type in typesInNamespace)
            {
                EventInfo ei = new EventInfo();
                ei.Type = type;
                ei.EventId = eventInfos.Count;
                eventInfos.Add(ei);
                AddChildGenerator(new ClassGenerator(ei.Type, ei.EventId));
            }
            AddChildGenerator(new EventFactoryGenerator(FactoryName, eventInfos));
            AddChildGenerator(new EventHandlersGenerator(HandlersName, eventInfos));
        }
        public override void Write(ICodeWriter o)
        {
            o.BeginBlock("namespace {0} {{", _namespace);
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
            o.EndBlock("}");
        }
        string FactoryName
        {
            get { return Prefix + "EventFactory"; }
        }
        string HandlersName
        {
            get { return Prefix + "EventHandlers"; }
        }
        string Prefix
        {
            get { return NamingHelper.GetPrefix(_namespace, "Events"); }
        }
        string _namespace;
    }
}
