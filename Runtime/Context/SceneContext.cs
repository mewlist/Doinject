using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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



        public override Scene Scene => Context.Scene;
        private SceneContextLoader ownerSceneContextLoader;
        private SceneContextLoader OwnerSceneContextLoader => ownerSceneContextLoader;


        private IContext ParentContext { get; set; }


        protected override async void Awake()
        {
            base.Awake();
            if (ContextSpaceScope.Scoped) return;
            var scene = gameObject.scene;
            await Boot(scene, parentContext: ProjectContext.Instance);
        }

        private async void OnDestroy()
        {
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
            await SceneContextLoader.UnloadAllScenesAsync();
            await Shutdown();
            await Resources.UnloadUnusedAssets();
            await ContextSpaceScope.WaitForRelease(destroyCancellationToken);
            await Boot(gameObject.scene, ParentContext);
        }

        private async Task Boot(Scene scene, IContext parentContext)
        {
            ParentContext = parentContext;

            Context = new Context(scene, parentContext?.Context);
            Context.Container.Bind<IContextArg>().FromInstance(Arg);

            if (GetComponentsUnderContext<SceneContext>().Any(x => x != this))
                throw new InvalidOperationException("There are multiple SceneContexts in the same scene.");

            ownerSceneContextLoader = parentContext?.SceneContextLoader;

            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);

            GameObjectContextLoader = gameObject.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);

            SceneContextMap[scene] = this;

            InstallBindings();
            await Context.Container.GenerateResolvers();
            await InjectIntoUnderContextObjects();
        }

        private async Task Shutdown()
        {
            if (SceneContextLoader) await SceneContextLoader.DisposeAsync();
            if (GameObjectContextLoader) await GameObjectContextLoader.DisposeAsync();
            if (Context is not null) await Context.DisposeAsync();
        }

        private void InstallBindings()
        {
            Context.Container.Bind<IContext>().FromInstance(this);
            Context.Container.BindFromInstance(SceneContextLoader);
            Context.Container.BindFromInstance(GameObjectContextLoader);
            var installers = GetComponentsUnderContext<IBindingInstaller>().ToList();
            Context.Install(installers, Arg);
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