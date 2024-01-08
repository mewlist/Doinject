using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class MonoBehaviourBindingTest
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
        public async Task BasicBindingTest()
        {
            container.Bind<InjectedObject>();
            container.Bind<TestMonoBehaviour>();

            var instance = await container.ResolveAsync<TestMonoBehaviour>();
            Assert.IsTrue(instance);
            Assert.IsTrue(instance.gameObject);
            Assert.IsFalse(instance.transform.parent, "Instantiate under scene root");
        }

        [Test]
        public async Task MonoBehaviourBindingWithArgsTest()
        {
            container.Bind<InjectedObject>();
            container.Bind<TestMonoBehaviourWithArgs>()
                .UnderSceneRoot()
                .Args(999, "fuga")
                .AsCached();

            var instance = await container.ResolveAsync<TestMonoBehaviourWithArgs>();
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.Arg1, Is.EqualTo(999));
            Assert.That(instance.Arg2, Is.EqualTo("fuga"));
            Assert.IsFalse(instance.transform.parent, "Instantiate under scene root");
        }

        [Test]
        public async Task MonoBehaviourBindingSingletonTest()
        {
            var go = new GameObject("MonoBehaviourBindingSingletonTest");
            container.Bind<InjectedObject>();
            container.Bind<TestMonoBehaviour>()
                .Under(go.transform)
                .AsSingleton();
            await container.GenerateResolvers();
            var targetTypeInfo = new TargetTypeInfo(typeof(TestMonoBehaviour));
            Assert.That(container.ReadOnlyInstanceMap[targetTypeInfo].Any(), Is.True);

            var instanceA = await container.ResolveAsync<TestMonoBehaviour>();
            var instanceB = await container.ResolveAsync<TestMonoBehaviour>();
            Assert.IsTrue(instanceA);
            Assert.IsTrue(instanceA.gameObject);
            Assert.That(instanceA == instanceB, Is.True);
            Assert.That(go.GetComponentInChildren<TestMonoBehaviour>(), Is.InstanceOf<TestMonoBehaviour>());
            Assert.That(instanceA.transform.parent, Is.EqualTo(go.transform));
        }

        [Test]
        public async Task MonoBehaviourBindingOnGameObjectTest()
        {
            var targetGameObject = new GameObject();
            container.Bind<InjectedObject>();
            container
                .Bind<TestMonoBehaviour>()
                .On(targetGameObject)
                .AsTransient();

            var instance = await container.ResolveAsync<TestMonoBehaviour>();
            Assert.IsTrue(instance);
            Assert.IsTrue(instance.gameObject);
            Assert.IsTrue(targetGameObject.GetComponent<TestMonoBehaviour>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task MonoBehaviourBindingUnderTransformTest(bool worldPositionStays)
        {
            var parentGameObject = new GameObject
                { transform = { position = new Vector3(1000, 0, 0) } };
            container.Bind<InjectedObject>();
            container
                .Bind<TestMonoBehaviour>()
                .Under(parentGameObject.transform, worldPositionStays);

            var instance = await container.ResolveAsync<TestMonoBehaviour>();
            Assert.IsFalse(parentGameObject.GetComponent<TestMonoBehaviour>());
            Assert.IsTrue(parentGameObject.GetComponentInChildren<TestMonoBehaviour>());
            Assert.IsTrue(instance.transform.parent.gameObject == parentGameObject);

            Assert.That(instance.transform.position, worldPositionStays
                    ? Is.EqualTo(new Vector3(0, 0, 0))
                    : Is.EqualTo(new Vector3(1000, 0, 0)));
        }

        [Test]
        public async Task FluentInterfaceTest()
        {
            var injectedObject = new InjectedObject();

            container
                .Bind<InjectedObject>()
                .FromInstance(injectedObject);

            container
                .Bind<MonoBehaviour>()
                .To<TestMonoBehaviour>()
                .AsCached();

            Assert.IsTrue(container.HasBinding<InjectedObject>());
            Assert.IsTrue(container.HasBinding<MonoBehaviour>());

            var instance = await container.ResolveAsync<InjectedObject>();
            var monoBehaviourInstanceA = await container.ResolveAsync<MonoBehaviour>();
            var monoBehaviourInstanceB = await container.ResolveAsync<MonoBehaviour>();

            Assert.NotNull(instance);
            Assert.NotNull(monoBehaviourInstanceA);
            Assert.That(monoBehaviourInstanceA is TestMonoBehaviour, Is.True);
            Assert.That(monoBehaviourInstanceA == monoBehaviourInstanceB, Is.True);
        }

        [Test]
        public void NotMonoBehaviourTest()
        {
            var go = new GameObject();
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                container.Bind<InjectedObject>()
                    .On(go);
            });
            container.Unbind<InjectedObject>();
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                container.Bind<InjectedObject>()
                    .Under(go.transform);
            });
            container.Unbind<InjectedObject>();
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                container.Bind<InjectedObject>()
                    .UnderSceneRoot();
            });
        }
    }
}