using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public sealed class GameObjectContext : AbstractContextComponent, IGameObjectContextRoot
    {
        private IContext ParentContext { get; set; }

        public override Scene Scene => Context.Scene;
        public override bool IsReverseLoaded => false;

        private bool loading;
        private bool loaded;
        public override bool Loaded => loaded;


        protected override async void Awake()
        {
            base.Awake();
            if (ContextSpaceScope.Scoped) return;
            var parentContext = FindParentContext();
            switch (parentContext)
            {
                case null:
                case SceneContext { Loaded: false }:
                    // This context will be loaded by the parent scene context.
                    return;
                default:
                    await Boot(FindParentContext());
                    break;
            }
        }

        private async void OnDestroy()
        {
            if (Context is null) return;
            loaded = false;
            await Shutdown();
            if (gameObject) Destroy(gameObject);
        }

        public async Task Initialize()
        {
            await Boot(ContextSpaceScope.CurrentContext);
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
            await ContextSpaceScope.WaitForRelease(destroyCancellationToken);
            await Boot(ParentContext);
        }

        private async Task Boot(IContext parentContext)
        {
            if (loading || loaded) return;
            loading = true;
            ParentContext = parentContext;

            ContextInternal = new ContextInternal(gameObject, ParentContext?.Context);
            ContextInternal.RawContainer.Bind<IContextArg>().FromInstance(Arg);

            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);

            GameObjectContextLoader = gameObject.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);

            InstallBindings();

            var injectableComponents
                = GetComponentsUnderContext<IInjectableComponent>().Where(x => x.enabled);

            await ContextInternal.RawContainer.GenerateResolvers();

            await Task.WhenAll(injectableComponents.Select(x
                => ContextInternal.RawContainer.InjectIntoAsync(x).AsTask()));

            loading = false;
            loaded = true;
        }

        public async Task Shutdown()
        {
            if (SceneContextLoader) await SceneContextLoader.DisposeAsync();
            if (GameObjectContextLoader) await GameObjectContextLoader.DisposeAsync();
            if (Context is not null) await Context.DisposeAsync();
            loaded = false;
        }

        private void InstallBindings()
        {
            ContextInternal.RawContainer.Bind<IContext>().FromInstance(this);
            ContextInternal.RawContainer.BindFromInstance(SceneContextLoader);
            ContextInternal.RawContainer.BindFromInstance(GameObjectContextLoader);
            var installers = GetComponentsUnderContext<IBindingInstaller>();
            Context.Install(installers, Arg);
        }

        protected override IEnumerable<T> GetComponentsUnderContext<T>()
        {
            var targetType = typeof(T);
            return transform.GetComponentsInChildren(targetType, true)
                .Where(x => x.GetComponentInParent<GameObjectContext>() == this)
                .Where(x => x != this)
                .Cast<T>();
        }

        private IContext FindParentContext()
        {
            if (transform.parent)
            {
                var parentContext = transform.parent.GetComponentInParent<GameObjectContext>();
                if (parentContext) return parentContext;
            }

            if (SceneContext.TryGetSceneContext(gameObject.scene, out var sceneContext))
            {
                return sceneContext;
            }

            return ProjectContext.Instance;
        }
    }
}