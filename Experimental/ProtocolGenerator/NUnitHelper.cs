using System;
using System.Reflection;
using NUnit.Framework;

namespace ProtocolGenerator
{
    internal static class PrivateAccessor
    {
        public static object GetField(object instance, string name)
        {
            Type t = instance.GetType();
            return GetFieldImpl(t, instance, name);
        }

        public static object GetStaticField(Type t, string name)
        {
            return GetFieldImpl(t, null, name);
        }

        private static object GetFieldImpl(Type t, object instance, string name)
        {
            FieldInfo f = t.GetField(name, BindingFlags.Instance
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Public
                                           | BindingFlags.Static);

            return f.GetValue(instance);
        }

        public static object ExecuteMethod(object instance, string name, params object[] paramList)
        {
            Type t = instance.GetType();
            return ExecuteMethodImpl(t, instance, name, paramList);
        }

        public static object ExecuteStaticMethod(Type t, string name, params object[] paramList)
        {
            return ExecuteMethodImpl(t, null, name, paramList);
        }

        private static object ExecuteMethodImpl(Type t, object instance, string name, object[] paramList)
        {
            Type[] paramTypes = GetParamTypes(paramList);

            MethodInfo m = t.GetMethod(name, BindingFlags.Instance
                                             | BindingFlags.NonPublic
                                             | BindingFlags.Public
                                             | BindingFlags.Static,
                                       null,
                                       paramTypes,
                                       null);

            return m.Invoke(instance, paramList);
        }

        private static Type[] GetParamTypes(object[] paramList)
        {
            Type[] paramTypes = new Type[paramList.Length];

            for (int i = 0; i < paramList.Length; i++)
                paramTypes[i] = paramList[i].GetType();
            return paramTypes;
        }
    }

    internal sealed class NonPublic
    {
        public int PrivateValue
        {
            get { return privateValue; }
        }

        public static int StaticPrivateValue
        {
            get { return staticPrivateValue; }
            set { staticPrivateValue = value; }
        }

        private int privateValue;
        private static int staticPrivateValue;

        private void Increment()
        {
            privateValue++;
        }

        private void SetValue(int value)
        {
            privateValue = value;
        }

        private static void StaticSetValue(Type[] types)
        {
            staticPrivateValue = -1;
        }
    }

    [TestFixture]
    public sealed class PrivateAccessorTestCase
    {
        [SetUp]
        public void SetUp()
        {
            nonPublic = new NonPublic();
            NonPublic.StaticPrivateValue = 0;
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
            Assert.AreEqual(0, nonPublic.PrivateValue);
            PrivateAccessor.ExecuteMethod(nonPublic, "Increment");
            Assert.AreEqual(1, nonPublic.PrivateValue);
        }

        [Test]
        public void WithArg()
        {
            Assert.AreEqual(0, nonPublic.PrivateValue);
            PrivateAccessor.ExecuteMethod(nonPublic, "SetValue", 1);
            Assert.AreEqual(1, nonPublic.PrivateValue);
        }

        [Test]
        public void StaticMethod()
        {
            Assert.AreEqual(0, NonPublic.StaticPrivateValue);
            PrivateAccessor.ExecuteStaticMethod(typeof (NonPublic), "StaticSetValue", new object[] {new Type[] {}});
            Assert.AreEqual(-1, NonPublic.StaticPrivateValue);
        }

        private NonPublic nonPublic;
    }
}