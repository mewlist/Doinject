using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class WithArgsObject : IAsyncDisposable
    {
        public InjectedObject InjectedObject { get; }
        public int Arg1 { get; }
        public string Arg2 { get; }
        public List<int> Arg3 { get; }

        [Inject]
        public WithArgsObject(int arg1, InjectedObject injectedObject, string arg2, List<int> arg3)
        {
            InjectedObject = injectedObject;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }
    }
}