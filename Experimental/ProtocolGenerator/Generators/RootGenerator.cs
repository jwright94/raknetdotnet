using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TestEvents;

namespace ProtocolGenerator.Generators
{
    internal sealed class RootGenerator : AbstractGenerator
    {
        public RootGenerator(Type[] allTypes)
        {
            if (!ContainsInSameNamespace(allTypes))
            {
                throw new SyntaxErrorException("Too many namespace. You can pass one namespace in one template file.");
            }

            Type[] allEventClasses = FindClassByAttribute(allTypes, typeof (SiteOfHandlingAttribute));
            Type protocolInfoClass = GetProtocolInfoClass(allTypes);

            AddChildGenerator(new NamespaceGenerator(protocolInfoClass.Namespace, protocolInfoClass, allEventClasses));
        }

        private static Type GetProtocolInfoClass(Type[] allTypes)
        {
            Type protocolInfoClass;
            Type[] protocolInfoClasses = FindClassByAttribute(allTypes, typeof (ProtocolInfoAttribute));

            if (protocolInfoClasses.Length != 1)
            {
                throw new SyntaxErrorException("No ProtocolInfo or too many ProtocolInfo class in template file.");
            }

            protocolInfoClass = protocolInfoClasses[0];
            return protocolInfoClass;
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

        private static bool ContainsInSameNamespace(Type[] types)
        {
            string namespaceName = null;
            foreach (Type type in types)
            {
                if (namespaceName == null)
                {
                    // this is first one.
                    namespaceName = type.Namespace;
                }
                else
                {
                    if (namespaceName != type.Namespace)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static Type[] FindClassByAttribute(Type[] types, Type attributeType)
        {
            List<Type> matched = new List<Type>();
            foreach (Type t in types)
            {
                bool isDefined = Attribute.IsDefined(t, attributeType);
                if (t.IsClass && isDefined)
                {
                    matched.Add(t);
                }
            }
            return matched.ToArray();
        }
    }

    [TestFixture]
    public sealed class RootGeneratorTestCase
    {
        [Test]
        public void FinalOutput()
        {
            Type[] types = new Type[] {typeof (SimpleEvent), typeof (ProtocolInfo)};
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