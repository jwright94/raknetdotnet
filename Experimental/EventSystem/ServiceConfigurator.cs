using System.Collections;
using Castle.Core;
using Castle.Core.Logging;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NUnit.Framework;

namespace EventSystem
{
    #region For Castle MicroKernel

    internal static class ServiceConfigurator
    {
        public static void Dispose()
        {
            container.Dispose();
        }

        #region Generics API.

        public static ServiceType Resolve<ServiceType>()
        {
            return container.Resolve<ServiceType>();
        }

        public static ServiceType Resolve<ServiceType>(string key)
        {
            return container.Resolve<ServiceType>(key);
        }

        public static ServiceType Resolve<ServiceType>(IDictionary arguments)
        {
            return (ServiceType) container.Kernel.Resolve(typeof (ServiceType), arguments);
        }

        public static ServiceType Resolve<ServiceType>(string key, IDictionary arguments)
        {
            return (ServiceType) container.Kernel.Resolve(key, arguments);
        }

        public static void RegisterCustomDependencies(string key, IDictionary dependencies)
        {
            container.Kernel.RegisterCustomDependencies(key, dependencies);
        }

        public static void RegisterCustomDependencies<ServiceType>(IDictionary dependencies)
        {
            container.Kernel.RegisterCustomDependencies(typeof (ServiceType), dependencies);
        }

        public static void AddComponent<ClassType>()
        {
            AddComponent<ClassType>(typeof(ClassType).FullName);
        }

        public static void AddComponent<ClassType>(string key)
        {
            container.AddComponent(key, typeof(ClassType));
        }

        public static void AddComponent<ServiceType, ClassType>()
        {
            AddComponent<ServiceType, ClassType>(typeof (ServiceType).FullName);
        }

        public static void AddComponent<ServiceType, ClassType>(string key)
        {
            container.AddComponent(key, typeof(ServiceType), typeof(ClassType));
        }

        public static void ReleaseComponent(object instance)
        {
            container.Release(instance);
        }

        #endregion

        public static IWindsorContainer Container
        {
            get { return container; }
        }

        public static ILoggerFactory LogFactory
        {
            get { return Resolve<ILoggerFactory>(); }
        }

        private static readonly IWindsorContainer container =
            new WindsorContainer(new XmlInterpreter("WindsorConfig.xml"));
    }

    #region Unit Tests

    namespace Tests
    {
        internal abstract class AbstractCounter
        {
            public AbstractCounter()
            {
                count = 0;
            }

            public void Increment()
            {
                count++;
            }

            public int Count
            {
                get { return count; }
            }

            protected int count;
        }

        [Singleton]
        internal class SingletonUsesAtAnyTime : AbstractCounter
        {
        }

        [Singleton]
        internal class ResettableSingleton : AbstractCounter
        {
            public void Reset()
            {
                count = 0;
            }
        }

        [Transient]
        internal class CountIncrementor
        {
            public CountIncrementor(SingletonUsesAtAnyTime singleton)
            {
                singleton.Increment();
            }
        }

        [TestFixture]
        public sealed class MicroKernelSingletonTestCase
        {
            [SetUp]
            public void SetUp()
            {
                container = new WindsorContainer(new XmlInterpreter("WindsorConfig.xml"));
                container.AddComponent("atanytime", typeof (SingletonUsesAtAnyTime));
                container.AddComponent("anotherinstance", typeof(SingletonUsesAtAnyTime));
                container.AddComponent("resettable", typeof (ResettableSingleton));
                container.AddComponent("incrementor", typeof (CountIncrementor));
            }

            [TearDown]
            public void TearDown()
            {
                container.Dispose();
            }

            [Test]
            public void SameInstance()
            {
                SingletonUsesAtAnyTime first = container.Resolve<SingletonUsesAtAnyTime>();
                SingletonUsesAtAnyTime second = container.Resolve<SingletonUsesAtAnyTime>();
                Assert.AreSame(first, second);
            }

            [Test]
            public void TwoInstanceOfSingleton()
            {
                SingletonUsesAtAnyTime atAnyTime = container.Resolve<SingletonUsesAtAnyTime>("atanytime");
                SingletonUsesAtAnyTime anotherInstance = container.Resolve<SingletonUsesAtAnyTime>("anotherinstance");
                Assert.AreNotSame(atAnyTime, anotherInstance);
            }

            [Test]
            public void HoldingCount()
            {
                SingletonUsesAtAnyTime first = container.Resolve<SingletonUsesAtAnyTime>();
                Assert.AreEqual(first.Count, 0);
                first.Increment();
                Assert.AreEqual(first.Count, 1);

                SingletonUsesAtAnyTime second = container.Resolve<SingletonUsesAtAnyTime>();
                Assert.AreEqual(second.Count, 1);
            }

            [Test]
            public void ResetCount()
            {
                ResettableSingleton first = container.Resolve<ResettableSingleton>();
                first.Increment();
                Assert.AreEqual(first.Count, 1);

                ResettableSingleton second = container.Resolve<ResettableSingleton>();
                Assert.AreEqual(second.Count, 1);
                second.Reset();

                Assert.AreEqual(first.Count, 0);
                Assert.AreEqual(second.Count, 0);
            }

            [Test]
            public void Dependency()
            {
                SingletonUsesAtAnyTime singleton = container.Resolve<SingletonUsesAtAnyTime>();
                Assert.AreEqual(singleton.Count, 0);
                CountIncrementor incrementor = container.Resolve<CountIncrementor>();
                Assert.AreEqual(singleton.Count, 1);
            }

            private IWindsorContainer container;
        }

        [TestFixture]
        public sealed class ServiceConfiguratorTestCase
        {
            [Test]
            public void ResolveProcessorOnNamingServer()
            {
                IProtocolProcessor processor = ServiceConfigurator.Resolve<IProtocolProcessor>("namingserver.processor");
                Assert.IsNotNull(processor);
            }
        }
    }

    #endregion

    #endregion
}