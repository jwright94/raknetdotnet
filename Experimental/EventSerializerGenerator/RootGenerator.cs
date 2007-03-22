using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace EventSerializerGenerator
{
    sealed class RootGenerator : AbstractGenerator
    {
        public RootGenerator(Assembly templateAssembly)
        {
            this.templateAssembly = templateAssembly;
            Type[] allTypes = this.templateAssembly.GetTypes();
            IDictionary<string, ICollection<Type>> namespaceTypesHash = new Dictionary<string, ICollection<Type>>();
            foreach (Type type in allTypes)
            {
                ICollection<Type> typesInNamespace;
                if (!namespaceTypesHash.TryGetValue(type.Namespace, out typesInNamespace))
                {
                    typesInNamespace = new List<Type>();
                    namespaceTypesHash[type.Namespace] = typesInNamespace;
                }
                typesInNamespace.Add(type);
            }
            foreach (KeyValuePair<string, ICollection<Type>> namespaceTypes in namespaceTypesHash)
            {
                AddChildGenerator(new NamespaceGenerator(namespaceTypes.Key, namespaceTypes.Value));
            }
        }
        public override void Write(ICodeWriter o)
        {
            o.WriteLine("using System;");
            o.WriteLine("using System.Collections.Generic;");
            o.WriteLine("using System.Text;");
            o.WriteLine("using RakNetDotNet;");
            o.WriteLine("using EventSystem;");
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
        }
        Assembly templateAssembly;
    }
}
