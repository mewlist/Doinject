using System;
using System.Threading.Tasks;

namespace Doinject
{
    internal static class Disposer
    {
        public static async ValueTask Dispose(object instance)
        {
            switch (instance)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}