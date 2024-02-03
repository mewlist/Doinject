using System.Threading.Tasks;
using UnityEngine;

namespace Doinject
{
    internal class ParallelScope
    {
        public int Count { get; private set; }

        private AwaitableCompletionSource CompletionSource { get; } = new();

        public bool Processing => Count > 0;

        public void Begin()
        {
            if (Count == 0) CompletionSource.Reset();
            Count++;
        }

        public void End()
        {
            Count--;
            if (Count == 0) CompletionSource.TrySetResult();
        }

        public async ValueTask Wait()
        {
            if (Count == 0) return;
            await CompletionSource.Awaitable;
        }
    }
}