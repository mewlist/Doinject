using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.TaskHelpers;
using UnityEngine;

namespace Doinject
{
    public sealed class ParentSceneContextRequirement : MonoBehaviour
    {
        [field: SerializeField] public UnifiedScene Scene { get; private set; }

        private ISceneHandle handle;

        public async Task<IContext> ResolveParentContext(SceneContext current)
        {
            if (!enabled) return ProjectContext.Instance;

#if UNITY_EDITOR
            using var scope = new ParentContextLoadingScope(gameObject.scene);

            handle = await UnifiedSceneLoader.LoadAsync(Scene, destroyCancellationToken);
            var scene = await handle.GetScene();

            var sceneContext = scene.FindFirstObjectByType<SceneContext>();
            if (!sceneContext)
            {
                await UnifiedSceneLoader.UnloadAsync(handle);
                return ProjectContext.Instance;
            }

            while (!sceneContext.Loaded)
            {
                destroyCancellationToken.ThrowIfCancellationRequested();
                await TaskHelper.NextFrame();
            }

            sceneContext.SceneContextLoader.AddChild(current);
            return sceneContext;
#else
            return ProjectContext.Instance;
#endif
        }
    }
}