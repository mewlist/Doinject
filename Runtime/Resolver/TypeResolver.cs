using System;
using System.Linq;
using System.Threading.Tasks;

namespace Doinject
{
    public class TypeResolver<T, TInstance> : AbstractInternalResolver<T>, ICacheStrategy
        where TInstance : T
    {
        public object[] Args { get; }
        public CacheStrategy CacheStrategy { get; }

        public TypeResolver(object[] args, CacheStrategy cacheStrategy, InstanceBag instanceBag)
            : base(instanceBag)
        {
            Args = args;
            CacheStrategy = cacheStrategy;
        }

        public override async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            switch (CacheStrategy)
            {
                case CacheStrategy.Singleton: case CacheStrategy.Cached:
                    if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                        return (T)InstanceBag.OfType(TargetType).First();
                    break;
            }

            var instance = await Instantiate(container, args);
            if (CacheStrategy != CacheStrategy.Transient)
                InstanceBag.Add(TargetType, instance);
            return instance;
        }

        private async ValueTask<T> Instantiate(IReadOnlyDIContainer container, object[] args)
        {
            return await container.InstantiateAsync<TInstance>(Args ?? args);
        }

        public async Task TryCacheAsync(DIContainer container)
        {
            if (CacheStrategy == CacheStrategy.Singleton)
                await ResolveAsync(container);
        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}