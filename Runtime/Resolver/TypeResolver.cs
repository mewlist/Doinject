using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Doinject
{
    public sealed class TypeResolver<T, TInstance> : AbstractInternalResolver<T>, ICacheStrategy
        where TInstance : T
    {
        private AwaitableCompletionSource CachingCompletionSource { get; set; }
        private object[] Args { get; }

        public CacheStrategy CacheStrategy { get; }
        public override string ShortName => "Type";
        public override string StrategyName => CacheStrategy.ToString();
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

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
                    if (CachingCompletionSource != null) await CachingCompletionSource.Awaitable;
                    if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                        return (T)InstanceBag.OfType(TargetType).First();
                    CachingCompletionSource = new AwaitableCompletionSource();
                    break;
                case CacheStrategy.Transient:
                default:
                    break;
            }

            var instance = await Instantiate(container, args);
            if (CacheStrategy != CacheStrategy.Transient)
                InstanceBag.Add(TargetType, instance);
            CachingCompletionSource?.TrySetResult();
            CachingCompletionSource = null;
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