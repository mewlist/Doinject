using System;
using System.Linq;
using System.Threading.Tasks;

namespace Doinject
{
    public sealed class CustomFactoryResolver<TFactory> : AbstractInternalResolver<TFactory>, IFactoryResolver
    {
        private object[] Args { get; }

        public TargetTypeInfo FactoryType { get; }

        public CustomFactoryResolver(InstanceBag instanceBag, object[] args) : base(instanceBag)
        {
            FactoryType = new TargetTypeInfo(typeof(TFactory));
            Args = args;
        }

        public override async ValueTask<TFactory> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            // always cached
            if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                return (TFactory)InstanceBag.OfType(TargetType).First();

            var instance = await container.InstantiateAsync<TFactory>(
                Args ?? args,
                Array.Empty<ScopedInstance>());
            InstanceBag.Add(TargetType, instance);
            return instance;
        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}