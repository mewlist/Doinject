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
        private List<GameObjectContext> ChildContexts { get; } = new();

        public void SetContext(IContext parentContext)
        {
            Context = parentContext;
        }

        public async ValueTask<T> LoadAsync<T>(T gameObjectContextPrefab, IContextArg arg = null)
            where T: GameObjectContext
        {
            var gameObjectContext = Instantiate(gameObjectContextPrefab);
            gameObjectContext.SetArgs(arg);
            using var instanceIds = new NativeArray<int>( new [] { gameObjectContext.gameObject.GetInstanceID() }, Allocator.Temp);
            SceneManager.MoveGameObjectsToScene(instanceIds, Context?.Scene ?? SceneManager.GetActiveScene());
            ChildContexts.Add(gameObjectContext);
            return gameObjectContext;
        }

        private void OnDestroy()
        {
            ChildContexts.ForEach(x=>
            {
                if (x) Destroy(x.gameObject);
            });
            ChildContexts.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            if (this) Destroy(this);
        }
    }
}