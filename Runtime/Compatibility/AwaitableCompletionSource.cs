using System;
using System.Threading.Tasks;

namespace Doinject
{
#if !UNITY_2023_2_OR_NEWER

    public class AwaitableCompletionSource
    {
        private TaskCompletionSource<bool> _taskCompletionSource = new();
        public Task Awaitable => _taskCompletionSource.Task;

        public void SetResult()
        {
            if (!TrySetResult())
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public void SetCanceled()
        {
            if (!TrySetCanceled())
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public void SetException(Exception exception)
        {
            if (!TrySetException(exception))
                throw new InvalidOperationException("Can't raise completion of the same Awaitable twice");
        }

        public bool TrySetResult() => _taskCompletionSource.TrySetResult(true);
        public bool TrySetCanceled() => _taskCompletionSource.TrySetCanceled();
        public bool TrySetException(Exception exception) => _taskCompletionSource.TrySetException(exception);
        public void Reset() => _taskCompletionSource = new TaskCompletionSource<bool>();
    }
#endif
}