using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class PrefabBindingTest
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

        private TestMonoBehaviour LoadPrefab()
        {
            const string prefabGuid = "b1f3d745bc6e3624b852543a31febb12";
            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            return AssetDatabase.LoadAssetAtPath<TestMonoBehaviour>(prefabPath);
        }

        [Test]
        public async Task CacheControlledPrefabBindingTest()
        {
            var prefab = LoadPrefab();

            container.Bind<InjectedObject>();
            container
                .BindPrefab<TestMonoBehaviour>(prefab)
                .AsTransient();
            container
                .BindPrefab<IInjectableComponent>(prefab.gameObject)
                .AsCached();

            var instanceA = await container.ResolveAsync<TestMonoBehaviour>();
            var instanceB = await container.ResolveAsync<TestMonoBehaviour>();
            var instanceC = await container.ResolveAsync<IInjectableComponent>();
            var instanceD = await container.ResolveAsync<IInjectableComponent>();
            Assert.AreNotSame(prefab, instanceA);
            Assert.AreNotSame(prefab, instanceB);
            Assert.AreSame(prefab.GetType(), instanceA.GetType());
            Assert.AreNotSame(instanceA, instanceB);
            Assert.AreNotSame(prefab, instanceC);
            Assert.AreSame(instanceC, instanceD);
        }

        [Test]
        public async Task PrefabBindingSingletonTest()
        {
            var prefab = LoadPrefab();

            container.Bind<InjectedObject>();
            container
                .BindPrefab<TestMonoBehaviour>(prefab)
                .AsSingleton();
            await container.GenerateResolvers();

            var instance = Object.FindFirstObjectByType<TestMonoBehaviour>();
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.transform.parent, Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task PrefabBindingUnderTransformTest(bool worldPositionStays)
        {
            var go = new GameObject("TestUnder");
            go.transform.position = new Vector3(1000, 0, 0);

            var prefab = LoadPrefab();

            container.Bind<InjectedObject>();
            container
                .BindPrefab<TestMonoBehaviour>(prefab)
                .Under(go.transform, worldPositionStays: worldPositionStays)
                .AsSingleton();

            await container.GenerateResolvers();
            var instance = Object.FindFirstObjectByType<TestMonoBehaviour>();
            Assert.That(instance.transform.parent, Is.EqualTo(go.transform));

            Assert.That(instance.transform.position, worldPositionStays
                ? Is.EqualTo(new Vector3(0, 0, 0))
                : Is.EqualTo(new Vector3(1000, 0, 0)));
        }
    }
}