using System.Threading;
using System.Threading.Tasks;
using Mew.Core.UnityObjectHelpers;
using Mew.Core.Tasks;
using UnityEngine;

namespace Doinject
{
    public class ContextEntryPoint : MonoBehaviour
    {
        private TaskQueue TaskQueue { get; } = new();
        private SceneContext SceneContext { get; set; }

        private async void Awake()
        {
            TaskQueue.Start();
            await StartContext();
        }

        private async void OnDestroy()
        {
            if (TaskQueue.Disposed) return;

            await TaskQueue.EnqueueAsync(_ =>
            {
                TaskQueue.Dispose();
                return Task.CompletedTask;
            });

            if (SceneContext) SceneContext.Dispose();
        }

        private async Task StartContext()
        {
            if (ContextSpaceScope.Scoped)
            {
                TaskQueue.Dispose();
                Destroy(this);
                return;
            }

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
            await Resources.UnloadUnusedAssets();
            await ContextSpaceScope.WaitForRelease(destroyCancellationToken);
            await StartContext();
        }
    }
}