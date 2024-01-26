using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class GameObjectContextLoader : MonoBehaviour, IAsyncDisposable
    {
        private IContext Context { get; set; }
        private HashSet<GameObjectContext> ChildContexts { get; } = new();
        public IReadOnlyCollection<GameObjectContext> ReadOnlyChildContexts => ChildContexts;

        public void SetContext(IContext parentContext)
        {
            Context = parentContext;
        }

        public ValueTask<T> LoadAsync<T>(T gameObjectContextPrefab, IContextArg arg = null)
            where T: GameObjectContext
        {
            var gameObjectContext = Instantiate(gameObjectContextPrefab);
            gameObjectContext.SetArgs(arg);
            using var instanceIds = new NativeArray<int>( new [] { gameObjectContext.gameObject.GetInstanceID() }, Allocator.Temp);
            SceneManager.MoveGameObjectsToScene(instanceIds, Context?.Scene ?? SceneManager.GetActiveScene());
            return new ValueTask<T>(gameObjectContext);
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

        public async ValueTask DisposeAsync()
        {
            if (this) Destroy(this);
        }
    }
}