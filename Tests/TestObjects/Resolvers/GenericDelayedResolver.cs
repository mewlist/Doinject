using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class GenericDelayedResolver<T> : IResolver<T>
        where T: new()
    {
        private T instance;
        public async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args)
        {
            await Task.Delay(1000);
            return instance ??= new T();
        }
    }
}