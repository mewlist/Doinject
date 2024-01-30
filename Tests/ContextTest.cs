using System.Threading.Tasks;
using Mew.Core.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class ContextTest
    {
        private const string TestSceneGuid = "ffbc51a464654db41b22dbd8fed68662";
        private const string TestGoContextPrefabGuid = "477c61f992dc83d469fecf95f27c67ec";

        private EditorBuildSettingsScene[] originalScenes;
        private string testScenePath;
        private string testGameObjectContextPrefabPath;

        [SetUp]
        public void Setup()
        {
            testScenePath = AssetDatabase.GUIDToAssetPath(TestSceneGuid);
            testGameObjectContextPrefabPath = AssetDatabase.GUIDToAssetPath(TestGoContextPrefabGuid);

        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task CreateSceneContextTest()
        {
            var go = new GameObject();
            var contextLoader = go.AddComponent<SceneContextLoader>();
            var sceneContext = await contextLoader.LoadAsync(new UnifiedScene { EditorScenePath = testScenePath }, true);

            Assert.That(SceneManager.loadedSceneCount, Is.EqualTo(2));
            Assert.That(Object.FindFirstObjectByType<SceneContext>(), Is.EqualTo(sceneContext));
            await contextLoader.UnloadAllScenesAsync();
            Assert.That(SceneManager.loadedSceneCount, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateGameObjectContextTest()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(testGameObjectContextPrefabPath);
            var goContextPrefab = prefab.GetComponent<GameObjectContext>();

            var go = new GameObject();
            var goContextLoader = go.AddComponent<GameObjectContextLoader>();

            var goContext = await goContextLoader.LoadAsync(goContextPrefab);

            Assert.That(SceneManager.loadedSceneCount, Is.EqualTo(1));
            Assert.That(Object.FindFirstObjectByType<GameObjectContext>(), Is.EqualTo(goContext));

            var testComponent = Object.FindFirstObjectByType<TestMonoBehaviour>();
            Assert.That(testComponent.InjectedObject, Is.Not.Null);
        }
    }
}