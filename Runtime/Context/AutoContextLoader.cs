using System.Collections.Generic;
using System.Threading.Tasks;
using Mew.Core.Assets;
using UnityEngine;

namespace Doinject
{
    public class AutoContextLoader : MonoBehaviour, IInjectableComponent
    {
        [SerializeField] private List<GameObjectContext> gameObjectContextPrefabs;
        [SerializeField] private List<UnifiedScene> sceneContexts;

        private IContext Context { get; set; }

        [Inject]
        // ReSharper disable once UnusedMember.Global
        public void Construct(IContext context)
        {
            Context = context;
        }

        public async Task OnInjected()
        {
            foreach (var gameObjectContextPrefab in gameObjectContextPrefabs)
                await Context.GameObjectContextLoader.LoadAsync(gameObjectContextPrefab);

            // If this context is loaded from child scene context, do not load child scene contexts.
            if (Context.IsReverseLoaded) return;

            foreach (var sceneContext in sceneContexts)
                await Context.SceneContextLoader.LoadAsync(sceneContext, active: true);
        }
    }
}