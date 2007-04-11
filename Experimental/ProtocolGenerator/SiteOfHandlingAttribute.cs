using System;

namespace ProtocolGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SiteOfHandlingAttribute : Attribute
    {
        public SiteOfHandlingAttribute(string site)
        {
            this.site = site;
            doesGenerateFactory = true;
            doesGenerateHandlers = true;
        }

        public string Site
        {
            get { return site; }
        }

        public bool DoesGenerateHandlers
        {
            get { return doesGenerateHandlers; }
            set { doesGenerateHandlers = value; }
        }

        public bool DoesGenerateFactory
        {
            get { return doesGenerateFactory; }
            set { doesGenerateFactory = value; }
        }

        private string site;
        private bool doesGenerateFactory;
        private bool doesGenerateHandlers;
    }
}