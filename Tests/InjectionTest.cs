using System;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class InjectionTest
    {
        private DIContainer container;

        [SetUp]
        public void Setup()
        {
            var scene = SceneManager.GetActiveScene();
            container = new DIContainer(parent: null, scene);
        }

        [TearDown]
        public async Task TearDown()
        {
            await container.DisposeAsync();
        }

        [Test]
        public async Task InstantiateTest()
        {
            var injectedObject = new InjectedObject();
            container.BindFromInstance(injectedObject);

            Assert.IsTrue(!container.HasBinding<ConstructorInjectionTestClass>());
            Assert.IsTrue(container.HasBinding<InjectedObject>());

            var instance = await container.InstantiateAsync<ConstructorInjectionTestClass>();
            Assert.NotNull(instance);
            Assert.AreSame(injectedObject, instance.InjectedObject);
        }

        [Test]
        public async Task ConstructorInjectionTest()
        {
            var injectedObject = new InjectedObject();
            container.BindTransient<ConstructorInjectionTestClass>();
            container.BindFromInstance(injectedObject);

            Assert.IsTrue(container.HasBinding<ConstructorInjectionTestClass>());
            Assert.IsTrue(container.HasBinding<InjectedObject>());

            var instance = await container.ResolveAsync<ConstructorInjectionTestClass>();
            Assert.NotNull(instance);
            Assert.AreSame(injectedObject, instance.InjectedObject);
        }

        [Test]
        public async Task MethodInjectionTest()
        {
            var injectedObject = new InjectedObject();
            container.BindTransient<MethodInjectionTestClass>();
            container.BindFromInstance(injectedObject);

            Assert.IsTrue(container.HasBinding<MethodInjectionTestClass>());
            Assert.IsTrue(container.HasBinding<InjectedObject>());

            var instance = await container.ResolveAsync<MethodInjectionTestClass>();

            Assert.NotNull(instance);
            Assert.AreSame(injectedObject, instance.InjectedObjectSync);
            Assert.AreSame(injectedObject, instance.InjectedObjectASync);
            Assert.AreSame(injectedObject, instance.InjectedObjectValueTask);
        }

        [Test]
        public void BindingNotFoundTest()
        {
            container.BindTransient<MethodInjectionTestClass>();
            Assert.IsTrue(container.HasBinding<MethodInjectionTestClass>());
            Assert.ThrowsAsync<FailedToResolveException>(async () =>
            {
                _ = await container.ResolveAsync<InjectedObject>();
            });
            Assert.ThrowsAsync<FailedToInjectException>(async () =>
            {
                _ = await container.ResolveAsync<MethodInjectionTestClass>();
            });
        }

        [Test]
        public void TypeValidationTest()
        {
            Assert.DoesNotThrow(() =>
            {
                container.BindTransient<TestSubClassObject>();
                container.BindTransient<TestImplementClassObject>();
                container.BindTransient<ITestInterface, TestImplementClassObject>();
                container.BindTransient<TestBaseClassObject, TestSubClassObject>();

                container.Unbind<TestSubClassObject>();
                container.Unbind<TestImplementClassObject>();
                container.Unbind<ITestInterface>();
                container.Unbind<TestBaseClassObject>();
            });

            Assert.Throws<Exception>(() =>
            {
                container.BindTransient<ITestInterface>();
            });
        }

        [Test]
        public async Task OptionalInjectionTest()
        {
            container.BindTransient<OptionalInjectionTestClass>();
            await container.ResolveAsync<OptionalInjectionTestClass>();
        }

        [Test]
        public async Task InjectedCallbackTest()
        {
            container.BindTransient<InjectedObject>();
            var instance = await container.ResolveAsync<InjectedObject>();
            Assert.That(instance.OnInjectedCalled, Is.False);
            while (container.AfterInjectProcessing)
                await TaskHelperInternal.NextFrame();
            await TaskHelperInternal.NextFrame();
            await TaskHelperInternal.NextFrame();
            Assert.That(instance.OnInjectedCalled, Is.True);
        }

        [Test]
        public async Task PropertyInjectionTest()
        {
            container.BindTransient<PropertyInjectionObject>();
            container.BindTransient<InjectedObject>();
            var instance = await container.ResolveAsync<PropertyInjectionObject>();
            Assert.NotNull(instance.InjectedObject);
        }

        [Test]
        public async Task PropertyInjectionComponentTest()
        {
            container.BindTransient<PropertyInjectionComponent>();
            container.BindTransient<InjectedObject>();
            var instance = await container.ResolveAsync<PropertyInjectionComponent>();
            Assert.NotNull(instance.InjectedObject);
        }

        [Test]
        public async Task PropertyInjectionWithNonPublicSetterComponentTest()
        {
            container.BindTransient<PropertyInjectionWithNonPublicSetterComponent>();
            container.BindTransient<InjectedObject>();
            try
            {
                await container.ResolveAsync<PropertyInjectionWithNonPublicSetterComponent>();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public async Task FieldInjectionTest()
        {
            container.BindTransient<FieldInjectionObject>();
            container.BindTransient<InjectedObject>();
            var instance = await container.ResolveAsync<FieldInjectionObject>();
            Assert.NotNull(instance.injectedObject);
        }

        [Test]
        public async Task FieldInjectionComponentTest()
        {
            container.BindTransient<FieldInjectionComponent>();
            container.BindTransient<InjectedObject>();
            var instance = await container.ResolveAsync<FieldInjectionComponent>();
            Assert.NotNull(instance.injectedObject);
        }

        [Test]
        public async Task FieldInjectionWithNonPublicSetterComponentTest()
        {
            container.BindTransient<FieldInjectionWithNonPublicObject>();
            container.BindTransient<InjectedObject>();
            try
            {
                await container.ResolveAsync<FieldInjectionWithNonPublicObject>();
            }
            catch (Exception _)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}