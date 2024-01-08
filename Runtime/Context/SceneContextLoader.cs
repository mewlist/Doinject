using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.UnityObjectHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class SceneContextLoader : MonoBehaviour, IAsyncDisposable
    {
        private SceneLoader SceneLoader { get; } = new();
        private SceneContext SceneContext { get; set; }
        private List<SceneContext> ChildSceneContexts { get; } = new();
        public IReadOnlyList<SceneContext> ReadonlyChildSceneContexts => ChildSceneContexts;

        private async void OnDestroy()
        {
            await UnloadAllScenesAsync();
            await SceneLoader.DisposeAsync();
        }

        public void SetContext(SceneContext sceneContext)
        {
            SceneContext = sceneContext;
        }

        public async Task<SceneContext> LoadAsync(SceneReference sceneReference, bool active)
        {
            return await LoadAsync(new UnifiedScene { SceneReference = sceneReference }, active);
        }

#if USE_DI_ADDRESSABLES
        public async Task<SceneContext> LoadAsync(SceneAssetReference sceneAssetReference, bool active)
        {
            return await LoadAsync(new UnifiedScene { SceneAssetReference = sceneAssetReference }, active);
        }
#endif

        public async ValueTask<SceneContext> LoadAsync(UnifiedScene unifiedScene, bool active, CancellationToken cancellationToken = default)
        {
            var scene = await SceneLoader.LoadAsync(unifiedScene, cancellationToken);
            if (!scene.IsValid()) return null;
            if (active) SceneManager.SetActiveScene(scene);

            var entryPoint = scene.FindFirstObjectByType<ContextEntryPoint>();
            if (entryPoint) return null;

            var sceneContext = UnityObjectHelper.InstantiateComponentInSceneRoot<SceneContext>(scene);
            sceneContext.transform.SetSiblingIndex(0);
            ChildSceneContexts.Add(sceneContext);
            await sceneContext.Initialize(scene, parentContext: SceneContext, sceneContextLoader: this);
            return sceneContext;
        }

        public async ValueTask UnloadAsync(SceneContext context)
        {
            if (!ChildSceneContexts.Contains(context)) return;

            foreach (var childSceneContext in ChildSceneContexts.ToArray())
                await childSceneContext.SceneContextLoader.UnloadAllScenesAsync();

            ChildSceneContexts.Remove(context);
            context.Dispose();
            await SceneLoader.UnloadAsync(context.Scene);
        }

        public async ValueTask UnloadAllScenesAsync()
        {
            await Task.WhenAll(ChildSceneContexts.ToArray().Select(x => UnloadAsync(x).AsTask()));
        }

        public async ValueTask DisposeAsync()
        {
            if (this) Destroy(this);
        }
    }
}