using System;

namespace EventSerializerGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SiteOfHandlingAttribute : Attribute
    {
        public SiteOfHandlingAttribute(string site)
        {
            this.site = site;
        }

        public string Site
        {
            get { return site; }
        }

        private string site;
    }
}