using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Doinject
{
    public sealed class FactoryResolver<T, TResolver, TFactory> : AbstractInternalResolver<TFactory>, IFactoryResolver
        where TFactory : IFactory
        where TResolver : IResolver<T>
    {
        private AwaitableCompletionSource CachingCompletionSource { get; set; }
        private TResolver InnerResolver { get; }
        public TargetTypeInfo FactoryType { get; }
        public override string ShortName => "Factory";
        public override string StrategyName => "Cached";
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

        public FactoryResolver(TResolver innerResolver, InstanceBag instanceBag)
            : base(instanceBag)
        {
            InnerResolver = innerResolver;
            FactoryType = new TargetTypeInfo(typeof(TFactory));
        }

        public override async ValueTask<TFactory> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            // always cached
            if (CachingCompletionSource != null) await CachingCompletionSource.Awaitable;
            if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                return (TFactory)InstanceBag.OfType(TargetType).First();
            CachingCompletionSource = new AwaitableCompletionSource();

            var resolverType = typeof(TResolver);
            var instance = await container.InstantiateAsync<TFactory>(
                args,
                new ScopedInstance[] { new (resolverType, InnerResolver) });
            InstanceBag.Add(TargetType, instance);
            CachingCompletionSource?.TrySetResult();
            CachingCompletionSource = null;
            return instance;
        }

        public override async ValueTask DisposeAsync()
        {
            if (InnerResolver is IInternalResolver internalResolver)
                await internalResolver.DisposeAsync();
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}