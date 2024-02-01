using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public sealed class ParentContextLoadingScope : System.IDisposable
    {
        private static List<Scene> StackedScope { get; set; } = new();

        public static bool Scoped => StackedScope.Any();
        public static Scene CurrentContext => StackedScope.Last();

        private readonly Scene scene;

        public ParentContextLoadingScope(Scene scene)
        {
            this.scene = scene;
            StackedScope.Add(scene);
        }

        public void Dispose()
        {
            StackedScope.Remove(scene);
        }

        public static async Task WaitForRelease(CancellationToken cancellationToken = default)
        {
            while (StackedScope.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await TaskHelper.NextFrame();
            }
        }
    }
}