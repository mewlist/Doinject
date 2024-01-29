using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;

namespace Doinject
{
    public sealed class ContextSpaceScope : System.IDisposable
    {
        private static List<IContext> StackedScope { get; set; } = new();

        public static bool Scoped => StackedScope.Any();

        private readonly IContext context;

        public ContextSpaceScope(IContext context)
        {
            this.context = context;
            StackedScope.Add(context);
        }

        public void Dispose()
        {
            StackedScope.Remove(context);
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