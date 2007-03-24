using System;
using System.Collections.Generic;

namespace EventSerializerGenerator
{
    internal sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string _namespace, IEnumerable<Type> typesInNamespace)
            // TODO: I forgot how to use simbol.
        {
            this._namespace = _namespace;
            IList<EventInfo> eventInfos = GetEventInfos(typesInNamespace);
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
            o.WriteLine("delegate void EventHandler<T>(T t) where T : ISimpleEvent;");
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

        private string BasicFactoryName
        {
            get { return Prefix + "EventFactory"; }
        }

        private string BasicHandlersName
        {
            get { return Prefix + "EventHandlers"; }
        }

        private string Prefix
        {
            get { return NamingHelper.GetPrefix(_namespace, "Events"); }
        }

        private string _namespace;
    }
}