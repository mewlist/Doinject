using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.Tasks;
using Mew.Core.UnityObjectHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public class SceneContextLoader : MonoBehaviour, IAsyncDisposable
    {
        private SceneLoader SceneLoader { get; } = new();
        private IContext Context { get; set; }
        private List<SceneContext> ChildSceneContexts { get; } = new();
        public IReadOnlyList<SceneContext> ReadonlyChildSceneContexts => ChildSceneContexts;
        private TaskQueue TaskQueue { get; } = new();
        private bool Disposed { get; set; }

        private void Awake()
        {
            TaskQueue.Start();
        }

        private async void OnDestroy()
        {
            Disposed = true;
            TaskQueue.Dispose();
            await UnloadAllScenesAsync();
            await SceneLoader.DisposeAsync();
        }

        public void SetContext(IContext context)
        {
            Context = context;
        }

        public async Task<SceneContext> LoadAsync(SceneReference sceneReference, bool active, IContextArg arg = null)
        {
            return await LoadAsync(new UnifiedScene { SceneReference = sceneReference }, active);
        }

#if USE_DI_ADDRESSABLES
        public async Task<SceneContext> LoadAsync(SceneAssetReference sceneAssetReference, bool active, IContextArg arg = null)
        {
            return await LoadAsync(new UnifiedScene { SceneAssetReference = sceneAssetReference }, active);
        }
#endif
        public async ValueTask<SceneContext> LoadAsync(UnifiedScene unifiedScene, bool active, IContextArg arg = null)
        {
            if (Disposed) return null;
            if (!unifiedScene.IsValid)
                throw new Exception("Cannot load scene. SceneReference is not valid.");
            var sceneContext = default(SceneContext);
            await TaskQueue.EnqueueAsync(async ct =>
            {
                sceneContext = await LoadAsyncInternal(unifiedScene, active, arg, ct);
            });
            return sceneContext;
        }

        private async ValueTask<SceneContext> LoadAsyncInternal(UnifiedScene unifiedScene, bool active,
            IContextArg arg, CancellationToken cancellationToken = default)
        {
            using var contextSpaceScope = new ContextSpaceScope(Context);

            var scene = await SceneLoader.LoadAsync(unifiedScene, cancellationToken);
            if (!scene.IsValid()) return null;
            if (active) SceneManager.SetActiveScene(scene);

            var sceneContext = UnityObjectHelper.InstantiateComponentInSceneRoot<SceneContext>(scene);
            sceneContext.SetArgs(arg);
            sceneContext.transform.SetSiblingIndex(0);
            ChildSceneContexts.Add(sceneContext);
            await sceneContext.Initialize(scene, parentContext: Context);
            return sceneContext;
        }

        public async ValueTask UnloadAsync(SceneContext context)
        {
            await TaskQueue.EnqueueAsync(async ct =>
            {
                await UnloadAsyncInternal(context);
            });
        }

        private async ValueTask UnloadAsyncInternal(SceneContext context)
        {
            var sceneName = context.Scene.name;
            if (!ChildSceneContexts.Contains(context)) return;

            foreach (var childSceneContext in ChildSceneContexts.ToArray())
                await childSceneContext.SceneContextLoader.UnloadAllScenesAsync();

            ChildSceneContexts.Remove(context);

            context.Dispose();

            await SceneLoader.UnloadAsync(context.Scene);
        }

        public async ValueTask UnloadAllScenesAsync()
        {
            if (Disposed) return;
            if (!ChildSceneContexts.Any()) return;

            await Task.WhenAll(ChildSceneContexts.ToArray().Select(x => UnloadAsync(x).AsTask()));
        }

        public ValueTask DisposeAsync()
        {
            if (this) Destroy(this);
            return new ValueTask();
        }
    }
}