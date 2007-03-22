using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NUnit.Framework;

namespace EventSerializerGenerator
{
    static class PrivateAccessor
    {
        public static object GetField(object instance, string name)
        {
            Type t = instance.GetType();

            FieldInfo f = t.GetField(name, BindingFlags.Instance
                                   | BindingFlags.NonPublic
                                   | BindingFlags.Public);

            return f.GetValue(instance);
        }
        public static object ExecuteMethod(object instance, string name, params object[] paramList)
        {
            Type t = instance.GetType();

            Type[] paramTypes = new Type[paramList.Length];

            for (int i = 0; i < paramList.Length; i++)
                paramTypes[i] = paramList[i].GetType();

            MethodInfo m = t.GetMethod(name, BindingFlags.Instance
                                     | BindingFlags.NonPublic
                                     | BindingFlags.Public,
                                    null,
                                    paramTypes,
                                    null);

            return m.Invoke(instance, paramList);
        }
    }
    sealed class NonPublic
    {
        public int GetPrivateValue()
        {
            return privateValue;
        }
        int privateValue;
        void Increment()
        {
            privateValue++;
        }
        void SetValue(int value)
        {
            privateValue = value;
        }
    }
    [TestFixture]
    public sealed class PrivateAccessorTestCase
    {
        [SetUp]
        public void SetUp()
        {
            nonPublic = new NonPublic();
        }
        [Test]
        public void GetField()
        {
            int privateValue = (int)PrivateAccessor.GetField(nonPublic, "privateValue");
            Assert.AreEqual(0, privateValue);
        }
        [Test]
        public void WithoutArg()
        {
            Assert.AreEqual(0, nonPublic.GetPrivateValue());
            PrivateAccessor.ExecuteMethod(nonPublic, "Increment");
            Assert.AreEqual(1, nonPublic.GetPrivateValue());
        }
        [Test]
        public void WithArg()
        {
            Assert.AreEqual(0, nonPublic.GetPrivateValue());
            PrivateAccessor.ExecuteMethod(nonPublic, "SetValue", 1);
            Assert.AreEqual(1, nonPublic.GetPrivateValue());
        }
        NonPublic nonPublic;
    }
}
