using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TestEvents;
using TestEvents2;

namespace ProtocolGenerator
{
    internal sealed class RootGenerator : AbstractGenerator
    {
        public RootGenerator(Type[] allTypes)
        {
            Type[] allEventClasses = GetEventAndProtocolClasses(allTypes);
            IDictionary<string, ICollection<Type>> namespaceTypesHash = GetNamespaceTypesHash(allEventClasses);
            foreach (KeyValuePair<string, ICollection<Type>> namespaceTypes in namespaceTypesHash)
            {
                AddChildGenerator(new NamespaceGenerator(namespaceTypes.Key, namespaceTypes.Value));
            }
        }

        public override void Write(ICodeWriter o)
        {
            WriteUsing(o);
            foreach (IGenerator generator in generators)
            {
                generator.Write(o);
            }
        }

        private static void WriteUsing(ICodeWriter o)
        {
            o.WriteLine("using System;");
            o.WriteLine("using System.Collections.Generic;");
            o.WriteLine("using System.Text;");
            o.WriteLine("using System.Diagnostics;");
            o.WriteLine("using Castle.Core;");
            o.WriteLine("using RakNetDotNet;");
            o.WriteLine("using EventSystem;");
        }

        private static IDictionary<string, ICollection<Type>> GetNamespaceTypesHash(Type[] allTypes)
        {
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
            return namespaceTypesHash;
        }

        //private static Type[] GetEventClasses(Type[] types)
        //{
        //    List<Type> filtered = new List<Type>();
        //    foreach (Type t in types)
        //    {
        //        bool isDefined = Attribute.IsDefined(t, typeof (SiteOfHandlingAttribute));
        //        if (t.IsClass && isDefined)
        //        {
        //            filtered.Add(t);
        //        }
        //    }
        //    return filtered.ToArray();
        //}

        private static Type[] GetEventAndProtocolClasses(Type[] types)
        {
            List<Type> filtered = new List<Type>();
            foreach (Type t in types)
            {
                bool isDefined = Attribute.IsDefined(t, typeof(SiteOfHandlingAttribute)) ||
                    Attribute.IsDefined(t, typeof(ProtocolInfoAttribute));
                if (t.IsClass && isDefined)
                {
                    filtered.Add(t);
                }
            }
            return filtered.ToArray();
        }       
    }

    [TestFixture]
    public sealed class RootGeneratorTestCase
    {
        [Test]
        public void ClassifyTypes()
        {
            Type[] types = new Type[] {typeof (SimpleEvent), typeof (SimpleEvent2)};
            IDictionary<string, ICollection<Type>> namespaceTypesHash =
                (IDictionary<string, ICollection<Type>>)
                PrivateAccessor.ExecuteStaticMethod(typeof(RootGenerator), "GetNamespaceTypesHash", new object[] { types });
            Assert.IsTrue(namespaceTypesHash.ContainsKey("TestEvents"));
            Assert.IsTrue(namespaceTypesHash.ContainsKey("TestEvents2"));
            Assert.IsTrue(namespaceTypesHash["TestEvents"].Contains(typeof (SimpleEvent)));
            Assert.IsTrue(namespaceTypesHash["TestEvents2"].Contains(typeof (SimpleEvent2)));
        }

        [Test]
        public void FinalOutput()
        {
            Type[] types = new Type[] {typeof (SimpleEvent)};
            IGenerator rootGenerator = new RootGenerator(types);
            TextWriter textWriter = new StringWriter();
            rootGenerator.Write(new CodeWriter(textWriter));
            using (StreamReader sr = File.OpenText("GeneratedTextSample.txt"))
            {
                Assert.AreEqual(sr.ReadToEnd(), textWriter.ToString());
            }
        }
    }
}