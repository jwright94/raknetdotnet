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
            get
            {
                string factoryName;
                int pos = _namespace.LastIndexOf("Events");
                if (0 < pos)
                {
                    factoryName = _namespace.Substring(0, pos);
                }
                else
                {
                    factoryName = _namespace;
                }
                factoryName += "EventFactory";
                return factoryName;
            }
        }
        string _namespace;
    }
}
