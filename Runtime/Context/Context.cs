using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class Context : IAsyncDisposable
    {
        public int Id { get; } = ContextTracker.Instance.GetNextId();
        public Scene Scene { get; }
        public GameObject GameObjectInstance { get; }
        public DIContainer Container { get; }
        public Context Parent { get; }

        public Context()
        {
            Scene = default;
            Parent = null;
            Container = new DIContainer(Parent?.Container, Scene);
            ContextTracker.Instance.Add(this);
        }

        public Context(Scene scene, Context parentContext)
        {
            Scene = scene;
            Parent = parentContext;
            Container = new DIContainer(Parent?.Container, Scene);
            ContextTracker.Instance.Add(this);
        }

        public Context(GameObject gameObjectInstance, Context parentContext)
        {
            Scene = parentContext?.Scene ?? gameObjectInstance.scene;
            GameObjectInstance = gameObjectInstance;
            Parent = parentContext;
            Container = new DIContainer(Parent?.Container, Scene);
            ContextTracker.Instance.Add(this);
        }


        public void Install(IEnumerable<IBindingInstaller> targets, IContextArg contextArg)
        {
            foreach (var component in targets)
                component.Install(Container, contextArg);
        }

        public void InstallScriptableObjects(
            List<BindingInstallerScriptableObject> targets)
        {
            foreach (var bindingScriptableObjectInstaller in targets)
                bindingScriptableObjectInstaller.Install(Container, new NullContextArg());
        }

        public async ValueTask DisposeAsync()
        {
            ContextTracker.Instance.Remove(this);
            await Container.DisposeAsync();
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