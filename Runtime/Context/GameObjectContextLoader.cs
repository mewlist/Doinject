using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Context
{
    public class GameObjectContextLoader : MonoBehaviour, IAsyncDisposable
    {
        private List<GameObjectContext> ChildContexts { get; } = new();

        public async ValueTask<T> LoadAsync<T>(IContext parentContext, T gameObjectContextPrefab, IContextArg arg = null)
            where T: GameObjectContext
        {
            var gameObjectContext = Instantiate(gameObjectContextPrefab);
            gameObjectContext.SetArgs(arg);
            using var instanceIds = new NativeArray<int>( new [] { gameObjectContext.gameObject.GetInstanceID() }, Allocator.Temp);
            SceneManager.MoveGameObjectsToScene(instanceIds, parentContext.Scene);
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