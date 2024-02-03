using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public class Context : IAsyncDisposable
    {
        protected readonly DIContainer container;

        public int Id { get; } = ContextTracker.Instance.GetNextId();
        public Scene Scene { get; }
        public GameObject GameObjectInstance { get; }
        public IReadOnlyDIContainer Container => container;
        public Context Parent { get; }
        public bool InjectionProcessing => container.InjectionProcessing;

        protected Context()
        {
            Scene = default;
            Parent = null;
            container = new DIContainer(Parent?.container, Scene);
        }

        protected Context(Scene scene, Context parentContext)
        {
            Scene = scene;
            Parent = parentContext;
            container = new DIContainer(Parent?.container, Scene);
        }

        protected Context(GameObject gameObjectInstance, Context parentContext)
        {
            Scene = parentContext?.Scene ?? gameObjectInstance.scene;
            GameObjectInstance = gameObjectInstance;
            Parent = parentContext;
            container = new DIContainer(Parent?.container, Scene);
        }


        public void Install(IEnumerable<IBindingInstaller> targets, IContextArg contextArg)
        {
            foreach (var component in targets)
                component.Install(container, contextArg);
        }

        public virtual async ValueTask DisposeAsync()
        {
            await container.DisposeAsync();
        }

        public override string ToString()
        {
            if (GameObjectInstance)
                return $"GameObjectContext: {GameObjectInstance.name}";
            else if (Scene.IsValid())
                return $"SceneContext: {Scene.name}";
            else
                return $"ProjectContext";
        }
    }
}