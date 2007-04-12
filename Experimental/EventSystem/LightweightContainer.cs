using System.Collections;
using Castle.Core;
using Castle.Core.Logging;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using NUnit.Framework;

namespace EventSystem
{
    // TODO - I should test setter injection with transient object.
    internal static class LightweightContainer
    {
        public static void Configure()
        {
            windsorContainer = new WindsorContainer();
        }

        public static void Configure(string filename)
        {
            windsorContainer = new WindsorContainer(new XmlInterpreter(filename));
        }

        public static void Dispose()
        {
            windsorContainer.Dispose();
        }

        #region Generics API.

        public static ServiceType Resolve<ServiceType>()
        {
            return windsorContainer.Resolve<ServiceType>();
        }

        public static ServiceType Resolve<ServiceType>(string key)
        {
            return windsorContainer.Resolve<ServiceType>(key);
        }

        public static ServiceType Resolve<ServiceType>(IDictionary arguments)
        {
            return (ServiceType)windsorContainer.Kernel.Resolve(typeof (ServiceType), arguments);
        }

        public static ServiceType Resolve<ServiceType>(string key, IDictionary arguments)
        {
            return (ServiceType)windsorContainer.Kernel.Resolve(key, arguments);
        }

        public static void RegisterCustomDependencies(string key, IDictionary dependencies)
        {
            windsorContainer.Kernel.RegisterCustomDependencies(key, dependencies);
        }

        public static void RegisterCustomDependencies<ServiceType>(IDictionary dependencies)
        {
            windsorContainer.Kernel.RegisterCustomDependencies(typeof (ServiceType), dependencies);
        }

        public static void AddComponent<ClassType>()
        {
            AddComponent<ClassType>(typeof (ClassType).FullName);
        }

        public static void AddComponent<ClassType>(string key)
        {
            windsorContainer.AddComponent(key, typeof (ClassType));
        }

        public static void AddComponent<ServiceType, ClassType>()
        {
            AddComponent<ServiceType, ClassType>(typeof (ServiceType).FullName);
        }

        public static void AddComponent<ServiceType, ClassType>(string key)
        {
            windsorContainer.AddComponent(key, typeof (ServiceType), typeof (ClassType));
        }

        public static void ReleaseComponent(object instance)
        {
            windsorContainer.Release(instance);
        }

        #endregion

        public static IWindsorContainer WindsorContainer
        {
            get { return windsorContainer; }
        }

        public static ILoggerFactory LogFactory
        {
            get { return Resolve<ILoggerFactory>(); }
        }

        private static IWindsorContainer windsorContainer;
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

        [Transient]
        internal sealed class Stateful
        {
            public string Message
            {
                get { return message; }
            }

            private readonly string message;

            public Stateful(string message)
            {
                this.message = message;
            }
        }

        [Transient]
        internal sealed class DependsStateful
        {
            private Stateful stateful;

            public Stateful Stateful
            {
                get { return stateful; }
            }

            // If we set stateful instance manually then we can't define setter property. Because DI container set undesirable instance automatically. 
            // And we don't recommend to use SetFoo naming rule.
            public void InitStateful(Stateful s)
            {
                stateful = s;
            }
        }

        [TestFixture]
        public sealed class MicroKernelSingletonTestCase
        {
            [SetUp]
            public void SetUp()
            {
                container = new WindsorContainer("config/test.xml");
                container.AddComponent("atanytime", typeof (SingletonUsesAtAnyTime));
                container.AddComponent("anotherinstance", typeof (SingletonUsesAtAnyTime));
                container.AddComponent("resettable", typeof (ResettableSingleton));
                container.AddComponent("incrementor", typeof (CountIncrementor));
                container.AddComponent("dependsstateful", typeof(DependsStateful));
            }

            [TearDown]
            public void TearDown()
            {
                container.Dispose();
            }

            [Test]
            public void SameSingleton()
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

            [Test]
            public void Stateful()
            {
                Stateful a = container.Resolve<Stateful>("statefula");
                Stateful b = container.Resolve<Stateful>("statefulb");
                Assert.AreNotEqual(a.Message, b.Message);
            }

            [Test]
            public void AvoidSetterInjection()
            {
                DependsStateful ds = container.Resolve<DependsStateful>();
                Assert.IsNull(ds.Stateful);
            }

            private IWindsorContainer container;
        }

        [TestFixture]
        public sealed class ServiceConfiguratorTestCase
        {
            [SetUp]
            public void SetUp()
            {
                LightweightContainer.Configure("config/common.xml");
            }

            [Test]
            public void ResolveProcessorRegistry()
            {
                IProcessorRegistry registry = LightweightContainer.Resolve<IProcessorRegistry>();
                Assert.IsNotNull(registry);
            }
        }
    }

    #endregion
}