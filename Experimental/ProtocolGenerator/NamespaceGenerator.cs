using System;
using System.Collections.Generic;

namespace ProtocolGenerator
{
    internal sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string _namespace, IEnumerable<Type> typesInNamespace)
            // TODO: I forgot how to use simbol.
        {
            this._namespace = _namespace;
            Attribute protocolAttribte;
            Type t = FindTypeWithProtocolAttribute(typesInNamespace, out protocolAttribte);


            IList<Type> eventTypes = new List<Type>(typesInNamespace);
            eventTypes.Remove(t);

            AddChildGenerator(new ProtocolInfoGenerator(t, protocolAttribte));
            IList<EventInfo> eventInfos = GetEventInfos(eventTypes);
            AddClassGenerators(eventInfos);
            AddHandlersGenerators(ClassifyBySite(eventInfos));
        }

        public override void Write(ICodeWriter o)
        {
            o.BeginBlock("namespace {0} {{", _namespace);
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
            WriteEventHandlerDelegate(o);
            o.EndBlock("}");
        }

        private static void WriteEventHandlerDelegate(ICodeWriter o)
        {
            o.WriteLine("delegate void EventHandler<T>(T t) where T : IEvent;");
        }

        private void AddClassGenerators(IList<EventInfo> eventInfos)
        {
            foreach (EventInfo ei in eventInfos)
            {
                AddChildGenerator(new ClassGenerator(ei.Type, ei.EventId));
            }
        }

        private void AddHandlersGenerators(IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite)
        {
            foreach (KeyValuePair<SiteOfHandlingAttribute, IList<EventInfo>> site in eventInfosBySite)
            {
                string OnSomewhere = "On" + site.Key.Site;
                AddChildGenerator(new EventFactoryGenerator(BasicFactoryName + OnSomewhere, site.Value));
                AddChildGenerator(new EventHandlersGenerator(BasicHandlersName + OnSomewhere, site.Value));
            }
        }

        private static Type FindTypeWithProtocolAttribute(IEnumerable<Type> typesInNamespace, out Attribute attributeOut)
        {
            attributeOut = null;
            List<Type> typeList = new List<Type>();
            foreach (Type type in typesInNamespace)
            {
                Attribute attribute = Attribute.GetCustomAttribute(type, typeof (ProtocolInfoAttribute));
                if (attribute != null)
                {
                    typeList.Add(type);
                    attributeOut = attribute;
                }
            }

            if (typeList.Count != 1)
            {
                throw new ApplicationException();
            }
            return typeList[0];
        }

        private static IList<EventInfo> GetEventInfos(IEnumerable<Type> typesInNamespace)
        {
            IList<EventInfo> eventInfos = new List<EventInfo>();
            foreach (Type type in typesInNamespace)
            {
                EventInfo ei = new EventInfo();
                ei.Type = type;
                ei.EventId = eventInfos.Count;
                eventInfos.Add(ei);
            }
            return eventInfos;
        }

        private static IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> ClassifyBySite(IList<EventInfo> eventInfos)
        {
            IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite =
                new Dictionary<SiteOfHandlingAttribute, IList<EventInfo>>();
            foreach (EventInfo ei in eventInfos)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(ei.Type, typeof (SiteOfHandlingAttribute));
                foreach (SiteOfHandlingAttribute siteAttribute in attributes)
                {
                    if (eventInfosBySite.ContainsKey(siteAttribute))
                    {
                        eventInfosBySite[siteAttribute].Add(ei);
                    }
                    else
                    {
                        IList<EventInfo> newList = new List<EventInfo>();
                        newList.Add(ei);
                        eventInfosBySite.Add(siteAttribute, newList);
                    }
                }
            }
            return eventInfosBySite;
        }

        private static string BasicFactoryName
        {
            get { return "EventFactory"; }
        }

        private static string BasicHandlersName
        {
            get { return "EventHandlers"; }
        }

        private string _namespace;
    }
}