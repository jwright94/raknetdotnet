using System;
using System.Collections.Generic;

namespace ProtocolGenerator.Generators
{
    internal sealed class NamespaceGenerator : AbstractGenerator
    {
        public NamespaceGenerator(string namespaceName, Type protocolInfoClass, uint minorVersion, IEnumerable<Type> eventClasses)
            // TODO: I forgot how to use simbol.
        {
            this.namespaceName = namespaceName;
            ProtocolInfoAttribute protocolInfoAttribute = (ProtocolInfoAttribute)Attribute.GetCustomAttribute(protocolInfoClass, typeof (ProtocolInfoAttribute));
            AddChildGenerator(new ProtocolInfoGenerator(protocolInfoClass, protocolInfoAttribute, minorVersion));
            IList<EventInfo> eventInfos = GetEventInfos(eventClasses);
            AddClassGenerators(protocolInfoClass, eventInfos);
            AddAuxiliaryGenerators(ClassifyBySite(eventInfos));
        }

        public override void Write(ICodeWriter o)
        {
            o.BeginBlock("namespace {0} {{", namespaceName);
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
            o.EndBlock("}");
        }

        private void AddClassGenerators(Type protocolInfoClass, IList<EventInfo> eventInfos)
        {
            foreach (EventInfo ei in eventInfos)
            {
                AddChildGenerator(new EventClassGenerator(ei.Type, ei.EventId, protocolInfoClass));
            }
        }

        private void AddAuxiliaryGenerators(IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite)
        {
            foreach (KeyValuePair<SiteOfHandlingAttribute, IList<EventInfo>> site in eventInfosBySite)
            {
                string OnSomewhere = "On" + site.Key.Site;
                if (site.Key.DoesGenerateFactory)
                {
                    AddChildGenerator(new EventFactoryGenerator(BasicFactoryName + OnSomewhere, site.Value));
                }
                if (site.Key.DoesGenerateHandlers)
                {
                    AddChildGenerator(new EventHandlersGenerator(BasicHandlersName + OnSomewhere, site.Value));
                }
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
            IDictionary<SiteOfHandlingAttribute, IList<EventInfo>> eventInfosBySite = new Dictionary<SiteOfHandlingAttribute, IList<EventInfo>>();
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

        private readonly string namespaceName;
    }
}