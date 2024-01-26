using System.Threading;
using System.Threading.Tasks;
using Mew.Core.UnityObjectHelpers;
using Mew.Core.Tasks;
using UnityEngine;

namespace Doinject.Context
{
    public class ContextEntryPoint : MonoBehaviour
    {
        private TaskQueue TaskQueue { get; } = new();
        public SceneContext SceneContext { get; set; }

        private async void Awake()
        {
            TaskQueue.Start();
            await StartContext();
        }

        private async void OnDestroy()
        {
            await TaskQueue.EnqueueAsync(ct =>
            {
                TaskQueue.Dispose();
                return Task.CompletedTask;
            });
            SceneContext.Dispose();
        }

        private async Task StartContext()
        {
            var scene = gameObject.scene;
            SceneContext = gameObject.AddComponent<SceneContext>();
            SceneContext.transform.SetSiblingIndex(0);
            await SceneContext.Initialize(scene, parentContext: ProjectContext.Instance);
        }


        public async void Reboot()
        {
            await TaskQueue.EnqueueAsync(async ct =>
            {
                await RebootInternal(ct);
            });
        }

        private async Task RebootInternal(CancellationToken ct)
        {
            if (SceneContext)
            {
                await SceneContext.SceneContextLoader.UnloadAllScenesAsync();
                await UnityObjectHelper.DestroyAsync(SceneContext);
            }
            Resources.UnloadUnusedAssets();
            await StartContext();
        }
    }
}