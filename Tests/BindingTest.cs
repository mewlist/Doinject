using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class BindingTest
    {
        private DIContainer container;

        [SetUp]
        public void Setup()
        {
            container = new DIContainer(parent: null, SceneManager.GetSceneAt(0));
        }

        [TearDown]
        public async Task TearDown()
        {
            await container.DisposeAsync();
        }

        [Test]
        public async Task InstanceBindingTest()
        {
            var instanceA = new InjectedObject();
            var instanceB = new InjectableObject(instanceA);
            container.BindFromInstance(instanceA);
            container.BindFromInstance(instanceB);
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            Assert.IsTrue(container.HasBinding(typeof(InjectedObject)));
            Assert.IsTrue(container.HasBinding<InjectableObject>());
            Assert.IsTrue(container.HasBinding(typeof(InjectableObject)));
            Assert.AreSame(instanceA, await container.ResolveAsync<InjectedObject>());
            Assert.AreSame(instanceB, await container.ResolveAsync<InjectableObject>());
        }

        [Test]
        public async Task TypeBindingTest()
        {
            container.BindTransient<InjectedObject>();
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            Assert.IsTrue(container.HasBinding(typeof(InjectedObject)));

            var instanceA = await container.ResolveAsync<InjectedObject>();
            var instanceB = await container.ResolveAsync<InjectedObject>();

            Assert.NotNull(instanceA);
            Assert.NotNull(instanceB);
            Assert.AreNotSame(instanceA, instanceB);
        }

        [Test]
        public async Task TypeBindingWithArgsTest()
        {
            container.BindTransient<InjectedObject>();
            container.Bind<WithArgsObject>()
                .Args(99, "hoge", new List<int> {1,2,3})
                .AsTransient();

            Assert.IsTrue(container.HasBinding<WithArgsObject>());

            var instance = await container.ResolveAsync<WithArgsObject>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.InjectedObject, Is.Not.Null);
            Assert.That(instance.Arg1, Is.EqualTo(99));
            Assert.That(instance.Arg2, Is.EqualTo("hoge"));
            CollectionAssert.AreEqual(instance.Arg3, new List<int> {1,2,3});
        }

        [Test]
        public async Task TypeBindingWithComplexArgsTest()
        {
            container.BindTransient<InjectedObject>();
            container.Bind<List<int>>()
                .FromInstance(new List<int> {1,2,3});
            container.Bind<WithArgsObject>()
                .Args(99, "hoge")
                .AsTransient();

            Assert.IsTrue(container.HasBinding<WithArgsObject>());

            var instance = await container.ResolveAsync<WithArgsObject>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.InjectedObject, Is.Not.Null);
            Assert.That(instance.Arg1, Is.EqualTo(99));
            Assert.That(instance.Arg2, Is.EqualTo("hoge"));
            CollectionAssert.AreEqual(instance.Arg3, new List<int> {1,2,3});
        }

        [Test]
        public async Task TypeBindingSingletonTest()
        {
            container.BindSingleton<InjectedObject>();
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            Assert.IsTrue(container.HasBinding(typeof(InjectedObject)));
            await container.GenerateResolvers();

            Assert.That(container.ReadOnlyInstanceMap[new TargetTypeInfo(typeof(InjectedObject))].Any(), Is.True);

            var instanceA = await container.ResolveAsync<InjectedObject>();
            var instanceB = await container.ResolveAsync<InjectedObject>();

            Assert.NotNull(instanceA);
            Assert.NotNull(instanceB);
            Assert.AreSame(instanceA, instanceB);
        }

        [Test]
        public async Task CachedBindingTest()
        {
            container.BindCached<InjectedObject>();
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            Assert.IsTrue(container.HasBinding(typeof(InjectedObject)));

            var tasks = new[]
            {
                container.ResolveAsync<InjectedObject>().AsTask(),
                container.ResolveAsync<InjectedObject>().AsTask()
            };
            var instances = await Task.WhenAll(tasks);
            Assert.NotNull(instances[0]);
            Assert.NotNull(instances[1]);
            Assert.AreSame(instances[0], instances[1]);
        }

        [Test]
        public async Task CustomResolverBindingTest()
        {
            container.Bind<ApiMock>();
            container.Bind<PlayerId>().FromInstance(new PlayerId("DummyId"));
            container.Bind<Player>().FromResolver<PlayerApiResolver>();
            var instance = await container.ResolveAsync<Player>();
            Assert.That(instance.PlayerId.Id, Is.EqualTo("DummyId"));
            Assert.That(instance.Name, Does.Contain("DummyId"));
        }

        [Test]
        public void AlreadyBoundTest()
        {
            Assert.Throws<Exception>(() =>
            {
                container.BindTransient<InjectedObject>();
                container.BindTransient<InjectedObject>();
            });
        }

        [Test]
        public void InterfaceCanNotInstantiateTest()
        {
            Assert.Throws<Exception>(() =>
            {
                container.BindTransient<IDisposable>();
            });
        }

        [Test]
        public async Task FailedToResolveTest()
        {
            try
            {
                container.Bind<InjectableObject>()
                    .AsSingleton();
                await container.ResolveAsync<InjectableObject>();
            }
            catch (FailedToCacheException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public async Task UnbindTest()
        {
            container.Bind<InjectedObject>();
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            container.Unbind<InjectedObject>();
            Assert.IsFalse(container.HasBinding<InjectedObject>());

            container.Bind<InjectedObject>();
            await container.ResolveAsync<InjectedObject>();
            Assert.IsTrue(container.HasBinding<InjectedObject>());
            container.Unbind<InjectedObject>();
            Assert.IsFalse(container.HasBinding<InjectedObject>());
            Assert.Throws<KeyNotFoundException>(() =>
            {
                container.Unbind<InjectedObject>();
            });
        }

        [Test]
        public async Task ParallelInjectionTest()
        {
            container.Bind<InjectedObject>().FromResolver<GenericDelayedResolver<InjectedObject>>();
            container.Bind<Player>().FromResolver<GenericDelayedResolver<Player>>();
            container.Bind<NovicePlayer>().FromResolver<GenericDelayedResolver<NovicePlayer>>();
            container.Bind<ComplexInjectableObject>();

            var sw = new Stopwatch();
            sw.Start();
            var instance = await container.ResolveAsync<ComplexInjectableObject>();
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000));
            Assert.That(instance.InjectedObjectA, Is.EqualTo(instance.InjectedObjectB));
            Assert.That(instance.PlayerFromProperty, Is.EqualTo(instance.PlayerFromMethod1));
            Assert.That(instance.PlayerFromProperty, Is.EqualTo(instance.PlayerFromMethod2));
            Assert.That(instance.NovicePlayerFromProperty, Is.EqualTo(instance.NovicePlayerFromMethod1));
            Assert.That(instance.NovicePlayerFromProperty, Is.EqualTo(instance.NovicePlayerFromMethod2));
        }
    }
}