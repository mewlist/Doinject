using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public class GameObjectContextLoader : MonoBehaviour, IAsyncDisposable
    {
        private IContext Context { get; set; }
        private HashSet<GameObjectContext> ChildContexts { get; } = new();
        public IReadOnlyCollection<GameObjectContext> ReadOnlyChildContexts => ChildContexts;

        public void SetContext(IContext context)
        {
            Context = context;
        }

        public async ValueTask<GameObjectContext> LoadAsync(GameObjectContext gameObjectContextPrefab, IContextArg arg = null)
        {
            using var contextSpaceScope = new ContextSpaceScope(Context);
            var gameObjectContext = Instantiate(gameObjectContextPrefab);
            gameObjectContext.SetArgs(arg);
            using var instanceIds = new NativeArray<int>( new [] { gameObjectContext.gameObject.GetInstanceID() }, Allocator.Temp);
            SceneManager.MoveGameObjectsToScene(instanceIds, Context?.Scene ?? SceneManager.GetActiveScene());
            Register(gameObjectContext);
            await gameObjectContext.Initialize();
            return gameObjectContext;
        }

        public void Register(GameObjectContext gameObjectContext)
        {
            if (ChildContexts.Contains(gameObjectContext))
                return;
            ChildContexts.RemoveWhere(x => !x);
            ChildContexts.Add(gameObjectContext);
        }

        public void Unregister(GameObjectContext gameObjectContext)
        {
            if (!ChildContexts.Contains(gameObjectContext))
                return;
            ChildContexts.Remove(gameObjectContext);
        }

        public void UnloadAllContextsAsync()
        {
            foreach (var x in ChildContexts)
                if (x) Destroy(x.gameObject);
            ChildContexts.Clear();
        }

        private void OnDestroy()
        {
            UnloadAllContextsAsync();
        }

        public ValueTask DisposeAsync()
        {
            if (this) Destroy(this);
            return new ValueTask();
        }
    }
}