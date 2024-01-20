using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class SceneContext : MonoBehaviour, IContext
    {
        private static readonly ConcurrentDictionary<Scene, SceneContext> SceneContextMap = new();

        public static bool TryGetSceneContext(Scene scene, out SceneContext sceneContext)
        {
            return SceneContextMap.TryGetValue(scene, out sceneContext);
        }

        public SceneContextLoader OwnerSceneContextLoader { get; set; }
        public Context Context { get; private set; }
        public SceneContextLoader SceneContextLoader { get; private set; }
        public Scene Scene => Context.Scene;

        public GameObject ContextObject => gameObject;

        public async Task Initialize(Scene scene, SceneContext parentContext, SceneContextLoader sceneContextLoader)
        {
            Context = new Context(scene, parentContext ? parentContext.Context : null);
            if (GetComponentsUnderContext<SceneContext>().Any(x => x != this))
                throw new InvalidOperationException("Do not place SceneContext statically in scene.");
            OwnerSceneContextLoader = sceneContextLoader;
            SceneContextLoader = gameObject.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);
            SceneContextMap[scene] = this;

            InstallBindings();
            await Context.Container.GenerateResolvers();
            await InjectIntoUnderContextObjects();
        }

        private async void OnDestroy()
        {
            if (SceneContextLoader) await SceneContextLoader.DisposeAsync();
            var scene = Context.Scene;
            await Context.DisposeAsync();
            if (OwnerSceneContextLoader) await OwnerSceneContextLoader.UnloadAsync(this);
            SceneContextMap.Remove(scene, out _);
        }

        private void InstallBindings()
        {
            Context.Container.Bind<IContext>().FromInstance(this);
            Context.Container.BindFromInstance(SceneContextLoader);
            var targets = GetComponentsUnderContext<IBindingInstaller>();
            foreach (var component in targets)
                component.Install(Context.Container);
        }

        private async Task InjectIntoUnderContextObjects()
        {
            var targets = GetComponentsUnderContext<IInjectableComponent>();
            await Task.WhenAll(targets.Select(x => Context.Container.InjectIntoAsync(x).AsTask()));
        }

        private IEnumerable<T> GetComponentsUnderContext<T>()
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

        public void Dispose()
        {
            if (this) Destroy(this);
        }
    }
}