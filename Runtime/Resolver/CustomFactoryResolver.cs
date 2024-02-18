using System;
using System.Linq;
using System.Threading.Tasks;
using Mew.Core;

namespace Doinject
{
    public sealed class CustomFactoryResolver<TFactory> : AbstractInternalResolver<TFactory>, IFactoryResolver
    {
        private MewCompletionSource CachingCompletionSource { get; set; }
        private object[] Args { get; }

        public TargetTypeInfo FactoryType { get; }
        public override string ShortName => "CustomFactory";
        public override string StrategyName => "Cached";
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

        public CustomFactoryResolver(InstanceBag instanceBag, object[] args) : base(instanceBag)
        {
            FactoryType = new TargetTypeInfo(typeof(TFactory));
            Args = args;
        }

        public override async ValueTask<TFactory> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            // always cached
            if (CachingCompletionSource != null) await CachingCompletionSource.Awaitable;
            if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                return (TFactory)InstanceBag.OfType(TargetType).First();
            CachingCompletionSource = new MewCompletionSource();

            var instance = await container.InstantiateAsync<TFactory>(
                Args ?? args,
                Array.Empty<ScopedInstance>());
            InstanceBag.Add(TargetType, instance);
            CachingCompletionSource?.TrySetResult();
            CachingCompletionSource = null;
            return instance;
        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}