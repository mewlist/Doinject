using System;
using System.Threading.Tasks;

namespace Doinject.Tests
{
    public class InjectedObject : IAsyncDisposable
    {
        public bool OnInjectedCalled { get; private set; }

        public InjectedObject()
        { }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        [OnInjected]
        public void OnInjected()
        {
            OnInjectedCalled = true;
        }
    }
}