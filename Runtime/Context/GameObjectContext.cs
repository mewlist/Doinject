using System.Linq;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine;

namespace Doinject.Context
{
    public class GameObjectContext : MonoBehaviour, IInjectableComponent, IContext
    {
        public Context Context { get; private set; }
        public SceneContextLoader OwnerSceneContextLoader => ParentContext.OwnerSceneContextLoader;
        private IContext ParentContext { get; set; }
        private bool Initialized { get; set; }

        public GameObject ContextObject => gameObject;

        public async Task Initialize()
        {
            if (Initialized) return;

            ParentContext = FindParentContext();
            if (ParentContext is null) return;

            Initialized = true;

            Context = new Context(gameObject, ParentContext.Context);
            await InstallBindings();
            await InjectIntoUnderContextObjects();
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
            await Context.DisposeAsync();
            if (gameObject) Destroy(gameObject);
        }

        private async Task InstallBindings()
        {
            if (!Initialized) await Initialize();
            Context.Container.Bind<IContext>().FromInstance(this);
            var targets = GetComponentsUnderContext<IBindingInstaller>();
            foreach (var component in targets)
                if (component is IBindingInstaller installer)
                    installer.Install(Context.Container);
        }

        private async Task InjectIntoUnderContextObjects()
        {
            var targets = GetComponentsUnderContext<IInjectableComponent>();
            foreach (var injectable in targets)
                await Context.Container.InjectIntoAsync(injectable);
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