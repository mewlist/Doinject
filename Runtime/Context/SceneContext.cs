using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public sealed class SceneContext : AbstractContextComponent
    {
        private static readonly ConcurrentDictionary<Scene, SceneContext> SceneContextMap = new();

        public static bool TryGetSceneContext(Scene scene, out SceneContext sceneSceneContext)
        {
            return SceneContextMap.TryGetValue(scene, out sceneSceneContext);
        }


        public override Scene Scene => gameObject.scene;

        private bool isReverseLoaded;
        public override bool IsReverseLoaded => isReverseLoaded;

        private bool loaded;
        public override bool Loaded => loaded;

        private SceneContextLoader ownerSceneContextLoader;
        private SceneContextLoader OwnerSceneContextLoader => ownerSceneContextLoader;


        private IContext ParentContext { get; set; }


        protected override async void Awake()
        {
            SceneContextMap[Scene] = this;

            base.Awake();

            if (ContextSpaceScope.Scoped) return;

            isReverseLoaded = ParentContextLoadingScope.Scoped;

            await Boot(Scene, await ResolveParentContext());
        }

        private async void OnDestroy()
        {
            isReverseLoaded = false;
            loaded = false;
            await Shutdown();
            if (Context is not null) SceneContextMap.Remove(Context.Scene, out _);
            if (OwnerSceneContextLoader) await OwnerSceneContextLoader.UnloadAsync(this);
        }

        public async Task Initialize(Scene scene, IContext parentContext)
        {
            await Boot(scene, parentContext);
        }

        public async void Reboot()
        {
            await TaskQueue.EnqueueAsync(async _
                => await RebootInternal());
        }

        private async Task RebootInternal()
        {
            isReverseLoaded = false;
            loaded = false;
            await SceneContextLoader.UnloadAllScenesAsync();
            await Shutdown();
            await Resources.UnloadUnusedAssets();
            await ContextSpaceScope.WaitForRelease(destroyCancellationToken);
            await Boot(gameObject.scene, ParentContext);
        }

        private async Task Boot(Scene scene, IContext parentContext)
        {
            ParentContext = parentContext;

            ContextInternal = new ContextInternal(scene, parentContext?.Context);
            ContextInternal.RawContainer.Bind<IContextArg>().FromInstance(Arg);

            if (GetComponentsUnderContext<SceneContext>().Any(x => x != this))
                throw new InvalidOperationException("There are multiple SceneContexts in the same scene.");

            ownerSceneContextLoader = parentContext?.SceneContextLoader;

            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);

            GameObjectContextLoader = gameObject.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);

            InstallBindings();

            var injectableComponents
                = GetComponentsUnderContext<IInjectableComponent>()
                    .Where(x => x.enabled);

            var gameObjectContexts
                = GetComponentsUnderContext<GameObjectContext>()
                    .Where(x => x.enabled);

            await ContextInternal.RawContainer.GenerateResolvers();

            await Task.WhenAll(injectableComponents.Select(x
                => ContextInternal.RawContainer.InjectIntoAsync(x).AsTask()));

            while (InjectionProcessing)
            {
                destroyCancellationToken.ThrowIfCancellationRequested();
                await TaskHelper.NextFrame();
            }

            using (new ContextSpaceScope(this))
            {
                foreach (var gameObjectContext in gameObjectContexts)
                    await gameObjectContext.Initialize();
            }
            loaded = true;
        }

        private async Task Shutdown()
        {
            if (SceneContextLoader) await SceneContextLoader.DisposeAsync();
            if (GameObjectContextLoader) await GameObjectContextLoader.DisposeAsync();
            if (Context is not null) await Context.DisposeAsync();
        }

        private void InstallBindings()
        {
            ContextInternal.RawContainer.Bind<IContext>().FromInstance(this);
            ContextInternal.RawContainer.BindFromInstance(SceneContextLoader);
            ContextInternal.RawContainer.BindFromInstance(GameObjectContextLoader);
            var installers = GetComponentsUnderContext<IBindingInstaller>().ToList();
            Context.Install(installers, Arg);
        }

        private async Task<IContext> ResolveParentContext()
        {
            var requirements = GetComponentsUnderContext<ParentSceneContextRequirement>().ToArray();

            if (!requirements.Any()) return ProjectContext.Instance;

            if (requirements.Length > 1)
                throw new Exception("There are multiple ParentSceneContextRequirements in the same scene.");

            var requirement = requirements.First();
            return await requirement.ResolveParentContext(this);
        }

        protected override IEnumerable<T> GetComponentsUnderContext<T>()
        {
            return Scene.FindComponentsByType(typeof(T))
                .Where(x =>
                {
                    if (x is GameObjectContext)
                    {
                        var parent = x.transform.parent;
                        return !parent || !parent.GetComponentInParent<GameObjectContext>();
                    }
                    return !x.GetComponentInParent<GameObjectContext>();
                })
                .Cast<T>();
        }
    }
}