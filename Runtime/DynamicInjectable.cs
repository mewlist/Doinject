using System.Linq;
using Mew.Core.TaskHelpers;
using UnityEngine;

namespace Doinject
{
    public class DynamicInjectable : MonoBehaviour, IInjectableComponent
    {
        private bool Injected { get; set; }

        [Inject]
        public void Construct()
        {
            Injected = true;
        }

        private async void Start()
        {
            if (Injected) return;

            var context = FindParentContext();
            while (!context.Loaded)
            {
                destroyCancellationToken.ThrowIfCancellationRequested();;
                await TaskHelper.NextFrame();
            }

            if (Injected) return;
            Injected = true;

            foreach (var component in GetComponents(typeof(IInjectableComponent)).Cast<IInjectableComponent>())
            {
                if (component == this as IInjectableComponent) continue;
                await context.Context.Container.InjectIntoAsync(component);
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

            return ProjectContext.Instance;
        }
    }
}