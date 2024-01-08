using System;
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
        public Context Parent { get; set; }
        public string SceneName { get; }

        public Context(Scene scene, Context parentContext)
        {
            Scene = scene;
            SceneName = Scene.name;
            Parent = parentContext;
            Container = new DIContainer(Parent?.Container, Scene);
            ContextTracker.Instance.Add(this);
        }

        public Context(GameObject gameObjectInstance, Context parentContext)
        {
            Scene = parentContext.Scene;
            SceneName = Scene.name;
            GameObjectInstance = gameObjectInstance;
            Parent = parentContext;
            Container = new DIContainer(Parent.Container, Scene);
            ContextTracker.Instance.Add(this);
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
            else
                return $"SceneContext: {SceneName}";
        }
    }
}