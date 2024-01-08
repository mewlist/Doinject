using System.Linq;
using System.Threading.Tasks;

namespace Doinject
{
    public class FactoryResolver<T, TResolver, TFactory> : AbstractInternalResolver<TFactory>, IFactoryResolver
        where TFactory : IFactory
        where TResolver : IResolver<T>
    {
        public TResolver InnerResolver { get; set; }
        public TargetTypeInfo FactoryType { get; }

        public FactoryResolver(TResolver innerResolver, InstanceBag instanceBag)
            : base(instanceBag)
        {
            InnerResolver = innerResolver;
            FactoryType = new TargetTypeInfo(typeof(TFactory));
        }

        public override async ValueTask<TFactory> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            // always cached
            if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                return (TFactory)InstanceBag.OfType(TargetType).First();

            var resolverType = typeof(TResolver);
            var instance = await container.InstantiateAsync<TFactory>(
                args,
                new ScopedInstance[] { new (resolverType, InnerResolver) });
            InstanceBag.Add(TargetType, instance);
            return instance;
        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}