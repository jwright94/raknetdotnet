using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using NUnit.Framework;

namespace EventSerializerGenerator
{
    sealed class RootGenerator : AbstractGenerator
    {
        public RootGenerator(Type[] allTypes)
        {
            IDictionary<string, ICollection<Type>> namespaceTypesHash = GetNamespaceTypesHash(allTypes);
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
        void WriteUsing(ICodeWriter o)
        {
            o.WriteLine("using System;");
            o.WriteLine("using System.Collections.Generic;");
            o.WriteLine("using System.Text;");
            o.WriteLine("using RakNetDotNet;");
            o.WriteLine("using EventSystem;");
        }
        IDictionary<string, ICollection<Type>> GetNamespaceTypesHash(Type[] allTypes)
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
    }

    [TestFixture]
    public sealed class RootGeneratorTestCase
    {
        [Test]
        public void ClassifyTypes()
        {
            Type[] types = new Type[] { typeof(TestEvents.SimpleEvent), typeof(TestEvents2.SimpleEvent2) };
            IGenerator rootGenerator = new RootGenerator(types);
            IDictionary<string, ICollection<Type>> namespaceTypesHash =
                (IDictionary<string, ICollection<Type>>)PrivateAccessor.ExecuteMethod(rootGenerator, "GetNamespaceTypesHash", (object)types);
            Assert.IsTrue(namespaceTypesHash.ContainsKey("TestEvents"));
            Assert.IsTrue(namespaceTypesHash.ContainsKey("TestEvents2"));
            Assert.IsTrue(namespaceTypesHash["TestEvents"].Contains(typeof(TestEvents.SimpleEvent)));
            Assert.IsTrue(namespaceTypesHash["TestEvents2"].Contains(typeof(TestEvents2.SimpleEvent2)));
        }
        [Test]
        public void FinalOutput()
        {
            Type[] types = new Type[] { typeof(TestEvents.SimpleEvent) };
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
