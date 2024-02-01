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



        protected override async void Awake()
        {
            base.Awake();
            if (ContextSpaceScope.Scoped) return;
            await Boot(FindParentContext());
        }

        private async void OnDestroy()
        {
            if (Context is null) return;
            await Shutdown();
            ParentContext?.GameObjectContextLoader.Unregister(this);
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
            ParentContext = parentContext;

            Context = new Context(gameObject, ParentContext?.Context);
            Context.Container.Bind<IContextArg>().FromInstance(Arg);

            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);

            GameObjectContextLoader = gameObject.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);

            ParentContext?.GameObjectContextLoader.Register(this);

            InstallBindings();

            var injectableComponents
                = GetComponentsUnderContext<IInjectableComponent>().Where(x => x.enabled);

            await Context.Container.GenerateResolvers();

            await Task.WhenAll(injectableComponents.Select(x
                => Context.Container.InjectIntoAsync(x).AsTask()));
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
                return sceneContext;

            return ProjectContext.Instance;
        }
    }
}