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
        void WriteEventHandlerDelegate(ICodeWriter o)
        {
            o.WriteLine("delegate void EventHandler<T>(T t) where T : ISimpleEvent;");
        }
        void AddClassGenerators(IList<EventInfo> eventInfos)
        {
            foreach (EventInfo ei in eventInfos)
            {
                AddChildGenerator(new ClassGenerator(ei.Type, ei.EventId));
            }
        }
        void AddHandlersGenerators(IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite)
        {
            foreach (KeyValuePair<SiteOfHandlingAttribute, IList<EventInfo>> site in eventInfosBySite)
            {
                string OnSomewhere = "On" + site.Key.Site;
                AddChildGenerator(new EventFactoryGenerator(BasicFactoryName + OnSomewhere, site.Value));
                AddChildGenerator(new EventHandlersGenerator(BasicHandlersName + OnSomewhere, site.Value));
            }
        }
        IList<EventInfo> GetEventInfos(IEnumerable<Type> typesInNamespace)
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
        IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> ClassifyBySite(IList<EventInfo> eventInfos)
        {
            IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite = new Dictionary<SiteOfHandlingAttribute, IList<EventInfo>>();
            foreach (EventInfo ei in eventInfos)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(ei.Type, typeof(SiteOfHandlingAttribute));
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
        string BasicFactoryName
        {
            get { return Prefix + "EventFactory"; }
        }
        string BasicHandlersName
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
