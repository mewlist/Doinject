using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Doinject
{
    public struct InstantiateMonoBehaviourBinder<T, TInstance> : IBinder
        where TInstance : T
    {
        private readonly BinderContext context;
        private object[] args;
        private readonly GameObject on;
        private readonly Transform under;
        private readonly bool worldPositionStays;

        private CacheStrategy cacheStrategy;

        public InstantiateMonoBehaviourBinder(BinderContext context, GameObject on)
        {
            context.ValidateInstanceType(typeof(TInstance));
            Assert.IsTrue(typeof(TInstance).IsSubclassOf(typeof(MonoBehaviour)));
            this.context = context;
            this.args = null;
            this.on = on;
            this.under = null;
            this.worldPositionStays = true;
            cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public InstantiateMonoBehaviourBinder(BinderContext context, Transform under, bool worldPositionStays)
        {
            context.ValidateInstanceType(typeof(TInstance));
            Assert.IsTrue(typeof(TInstance).IsSubclassOf(typeof(MonoBehaviour)));
            this.context = context;
            this.args = null;
            this.on = null;
            this.under = under;
            this.worldPositionStays = worldPositionStays;
            cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public InstantiateMonoBehaviourBinder<T, TInstance> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return under
                ? new MonoBahaviourResolver<T, TInstance>(args, cacheStrategy, instanceBag, under, worldPositionStays)
                : new MonoBahaviourResolver<T, TInstance>(args, cacheStrategy, instanceBag, on);
        }

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

        public FactoryBinder<T, Factory<T>> AsFactory()
        {
            return new FactoryBinder<T, Factory<T>>(context, AsTransient());
        }

        public FactoryBinder<T, TFactory> AsFactory<TFactory>()
            where TFactory : IFactory<T>
        {
            if (!typeof(Factory<T>).IsAssignableFrom(typeof(TFactory)) && typeof(TInstance) != typeof(T))
                throw new Exception("Custom factory cannot be used with custom instance type");
            return new FactoryBinder<T, TFactory>(context, AsTransient());
        }
    }
}