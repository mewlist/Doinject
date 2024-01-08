using System;

namespace Doinject
{
    public struct InstantiateBinder<T, TInstance> : IBinder
        where TInstance : T
    {
        private readonly BinderContext context;
        private object[] args;
        private CacheStrategy cacheStrategy;

        public InstantiateBinder(BinderContext context)
        {
            context.ValidateInstanceType(typeof(TInstance));
            this.context = context;
            this.args = null;
            this.cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public InstantiateBinder<T, TInstance> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
            => new TypeResolver<T, TInstance>(args, cacheStrategy, instanceBag);

        public FixedBinder AsCached()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Cached;
            context.Update(clone);
            return new FixedBinder(context, clone);
        }

        public FixedBinder AsTransient()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Transient;
            context.Update(clone);
            return new FixedBinder(context, clone);
        }

        public FixedBinder AsSingleton()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Singleton;
            context.Update(clone);
            return new FixedBinder(context, clone);
        }
    }
}