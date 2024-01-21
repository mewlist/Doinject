using System.Linq;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class GameObjectContext : MonoBehaviour, IInjectableComponent, IContext, IGameObjectContextRoot
    {
        public Scene Scene { get; private set; }
        public GameObject ContextObject => gameObject;
        public Context Context { get; private set; }
        public SceneContextLoader OwnerSceneContextLoader => ParentContext.SceneContextLoader;
        public SceneContextLoader SceneContextLoader { get; private set; }
        public GameObjectContextLoader GameObjectContextLoader { get; private set; }
        public IContextArg Arg { get; private set; } = new NullContextArg();

        private IContext ParentContext { get; set; }
        private bool Initialized { get; set; }

        public async Task Initialize()
        {
            if (Initialized) return;

            ParentContext = FindParentContext();
            if (ParentContext is null) return;

            Scene = ParentContext.Scene;
            Context = new Context(gameObject, ParentContext.Context);
            Context.Container.Bind<IContextArg>().FromInstance(Arg);
            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);
            GameObjectContextLoader = gameObject.AddComponent<GameObjectContextLoader>();
            Initialized = true;

            await InstallBindings();
            await Context.Container.GenerateResolvers();
            await InjectIntoUnderContextObjects();
        }

        public void SetArgs(IContextArg arg)
        {
            Arg = arg ?? new NullContextArg();
        }

        private async void Start()
        {
            while (!Initialized)
            {
                await TaskHelper.NextFrame();
                if (this) await Initialize();
            }
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

            return null;
        }

        private GameObjectContext[] FindChildContexts()
        {
            return transform.GetComponentsInChildren<GameObjectContext>(true)
                .Where(x => x != this)
                .Where(x => x.transform.parent && x.transform.parent.GetComponentInParent<GameObjectContext>() == this)
                .ToArray();
        }

        private async void OnDestroy()
        {
            if (Context is null) return;
            if (SceneContextLoader) await SceneContextLoader.DisposeAsync();
            if (GameObjectContextLoader) await GameObjectContextLoader.DisposeAsync();
            await Context.DisposeAsync();
            if (gameObject) Destroy(gameObject);
        }

        private async Task InstallBindings()
        {
            if (!Initialized) await Initialize();
            Context.Container.Bind<IContext>().FromInstance(this);
            Context.Container.BindFromInstance(SceneContextLoader);
            Context.Container.BindFromInstance(GameObjectContextLoader);
            var targets = GetComponentsUnderContext<IBindingInstaller>();
            foreach (var component in targets)
                if (component is IBindingInstaller installer)
                    installer.Install(Context.Container, Arg);
        }

        private async Task InjectIntoUnderContextObjects()
        {
            var targets = GetComponentsUnderContext<IInjectableComponent>();
            await Task.WhenAll(targets.Select(x
                => !Context.Container.HasBinding(x.GetType())
                    ? Context.Container.InjectIntoAsync(x).AsTask()
                    : Task.CompletedTask));
        }

        private T[] GetComponentsUnderContext<T>()
        {
            var targetType = typeof(T);
            return transform.GetComponentsInChildren(targetType, true)
                .Where(x => x.GetComponentInParent<GameObjectContext>() == this)
                .Where(x => x != this)
                .Cast<T>()
                .ToArray();
        }

        public void Dispose()
        {
            if (this) Destroy(this);
        }
    }
}