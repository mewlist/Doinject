﻿using System.Threading.Tasks;

namespace Doinject
{
    public abstract class AbstractInternalResolver<T> : IInternalResolver<T>
    {
        protected InstanceBag InstanceBag { get; }
        protected TargetTypeInfo TargetType { get; }
        public virtual string Name => $"{GetType().Name}<{typeof(T).Name}>";
        public abstract string ShortName { get; }
        public abstract string StrategyName { get; }
        public abstract int InstanceCount { get; }

        protected AbstractInternalResolver(InstanceBag instanceBag)
        {
            TargetType = new TargetTypeInfo(typeof(T));
            InstanceBag = instanceBag;
        }

        public async ValueTask<object> ResolveAsObjectAsync(DIContainer container)
        {
            return await ResolveAsync(container);
        }

        public abstract ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null);

        public abstract ValueTask DisposeAsync();
    }
}