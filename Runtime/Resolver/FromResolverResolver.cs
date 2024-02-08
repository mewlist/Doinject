using System.Threading.Tasks;

namespace Doinject
{
    public sealed class FromResolverResolver<T, TResolver> : IInternalResolver<T>
        where TResolver : IResolver<T>, new()
    {
        private TResolver Resolver { get; }
        private object[] Args { get; }
        private bool Injected { get; set; }

        public string Name => $"{GetType().Name}<{typeof(T).Name}>";
        public string ShortName => "Resolver";
        public string StrategyName => "FromResolver";
        public int InstanceCount => 1;

        public FromResolverResolver(object[] args)
        {
            Resolver = new TResolver();
            Args = args;
        }

        public async ValueTask<object> ResolveAsObjectAsync(DIContainer container)
        {
            return await ResolveAsync(container);
        }

        public async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            if (!Injected) await container.InjectIntoAsync(Resolver, args);
            Injected = true;
            return await Resolver.ResolveAsync(container, Args);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}