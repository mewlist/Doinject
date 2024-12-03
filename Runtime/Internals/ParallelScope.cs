using System.Threading.Tasks;
using Mew.Core;

namespace Doinject
{
    internal class ParallelScope
    {
        public int Count { get; private set; }

        private TaskCompletionSource<bool> taskCompletionSource = new();

        public bool Processing => Count > 0;

        public void Begin()
        {
            if (Count == 0) taskCompletionSource = new TaskCompletionSource<bool>();
            Count++;
        }

        public void End()
        {
            Count--;
            if (Count == 0) taskCompletionSource.TrySetResult(true);
        }

        public void Cancel()
        {
            taskCompletionSource.TrySetCanceled();
            Count = 0;
        }

        public async ValueTask Wait()
        {
            if (Count == 0) return;
            await taskCompletionSource.Task;
        }
    }
}