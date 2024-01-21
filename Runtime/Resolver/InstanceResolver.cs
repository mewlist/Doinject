using System.Threading.Tasks;

namespace Doinject
{
    public class InstanceResolver<T> : IInternalResolver
    {
        private T Instance { get; set; }
        public bool Injected { get; set; }
        public string Name => $"{GetType().Name}<{typeof(T).Name}>";


        public InstanceResolver(T instance)
        {
            Instance = instance;
        }

        public async ValueTask<object> ResolveAsObjectAsync(DIContainer container)
        {
            return await ResolveAsync(container, null);
        }

        public async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            if (!Injected) await container.InjectIntoAsync(Instance, args);
            Injected = true;
            return Instance;
        }

        public ValueTask DisposeAsync()
        {
            Instance = default;
            return new ValueTask(Task.CompletedTask);
        }
    }
}