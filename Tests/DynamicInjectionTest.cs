using System.Linq;
using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.TaskHelpers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Doinject.Tests
{
    public class DynamicInjectionTest
    {
        private const string TestSceneGuid = "ffbc51a464654db41b22dbd8fed68662";
        private const string DynamicInjectionPrefabGuid = "a543bb667da81084f89df9c087d89e53";

        private EditorBuildSettingsScene[] originalScenes;
        private string testScenePath;
        private string testGameObjectContextPrefabPath;

        [SetUp]
        public async Task Setup()
        {
            testScenePath = AssetDatabase.GUIDToAssetPath(TestSceneGuid);
            testGameObjectContextPrefabPath = AssetDatabase.GUIDToAssetPath(DynamicInjectionPrefabGuid);

        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task DynamicInjectionCallCountTest()
        {
            var go = new GameObject();
            var contextLoader = go.AddComponent<SceneContextLoader>();
            var sceneContext = await contextLoader.LoadAsync(new UnifiedScene { EditorScenePath = testScenePath }, true);
            var instances = sceneContext.Scene.FindComponentsByType<TestMonoBehaviour>();
            var prefab = AssetDatabase.LoadAssetAtPath<TestMonoBehaviour>(testGameObjectContextPrefabPath);

            await TaskHelperInternal.NextFrame();

            foreach (var testMonoBehaviour in instances)
                Assert.That(testMonoBehaviour.InjectedCount, Is.EqualTo(1));

            // Instantiate dynamic injected object under GameObject context
            var parent = Object.FindFirstObjectByType<GameObjectContext>().transform;
            var instance = Object.Instantiate(prefab, parent);
            Assert.That(instance.InjectedCount, Is.EqualTo(0));
            await TaskHelperInternal.NextFrame();
            Assert.That(instance.InjectedCount, Is.EqualTo(1));
            await contextLoader.UnloadAllScenesAsync();
        }
    }
}