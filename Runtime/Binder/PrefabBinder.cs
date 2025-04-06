using UnityEngine;

namespace Doinject
{
    public struct PrefabBinder<T> : IBinder
    {
        private readonly BinderContext context;
        private readonly Object prefab;
        private object[] args;
        private Transform under;
        private bool worldPositionStays;

        private CacheStrategy cacheStrategy;

        public PrefabBinder(BinderContext context, Object prefab)
        {
            this.context = context;
            this.prefab = prefab;
            this.args = null;
            this.under = default;
            this.worldPositionStays = true;
            cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new PrefabResolver<T>(prefab, args, under, worldPositionStays, cacheStrategy, instanceBag);
        }

        public PrefabBinder<T> Args(params object[] value)
        {
            var clone = this;
            clone.args = value;
            clone.context.Update(clone);
            return clone;
        }

        public PrefabBinder<T> UnderSceneRoot()
        {
            under = null;
            context.Update(this);
            return this;
        }

        public PrefabBinder<T> Under(Transform targetTransform)
            => Under(targetTransform, worldPositionStays: true);

        public PrefabBinder<T> Under(Transform targetTransform, bool worldPositionStays)
        {
            under = targetTransform;
            this.worldPositionStays = worldPositionStays;
            context.Update(this);
            return this;
        }

        public PrefabBinder<T> AsCached()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Cached;
            context.Update(clone);
            return clone;
        }

        public PrefabBinder<T> AsTransient()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Transient;
            context.Update(clone);
            return clone;
        }

        public PrefabBinder<T> AsSingleton()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Singleton;
            context.Update(clone);
            return clone;
        }

        public FactoryBinder<T, Factory<T>> AsFactory()
        {
            return new FactoryBinder<T, Factory<T>>(context, AsTransient());
        }

        public FactoryBinder<T, Factory<TArg1, T>> AsFactory<TArg1>() => new(context, AsTransient());
        public FactoryBinder<T, Factory<TArg1, TArg2, T>> AsFactory<TArg1, TArg2>() => new(context, AsTransient());
        public FactoryBinder<T, Factory<TArg1, TArg2, TArg3, T>> AsFactory<TArg1, TArg2, TArg3>() => new(context, AsTransient());
        public FactoryBinder<T, Factory<TArg1, TArg2, TArg3, TArg4, T>> AsFactory<TArg1, TArg2, TArg3, TArg4>() => new(context, AsTransient());
    }
}