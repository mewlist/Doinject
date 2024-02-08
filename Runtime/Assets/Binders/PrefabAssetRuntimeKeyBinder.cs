#if USE_DI_ASSETS
using System;
using UnityEngine;

namespace Doinject.Assets
{
    public struct PrefabAssetRuntimeKeyBinder<T> : IBinder
    {
        private readonly BinderContext context;
        private readonly object prefabAssetRuntimeKey;
        private object[] args;

        private Transform under;
        private bool worldPositionStays;
        private CacheStrategy cacheStrategy;

        public PrefabAssetRuntimeKeyBinder(BinderContext context, object prefabAssetRuntimeKey)
        {
            this.context = context;
            this.prefabAssetRuntimeKey = prefabAssetRuntimeKey;
            this.args = null;
            this.under = default;
            this.worldPositionStays = true;
            cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new PrefabAssetReferenceResolver<T>(prefabAssetRuntimeKey, args, under, worldPositionStays, cacheStrategy, instanceBag);
        }

        public PrefabAssetRuntimeKeyBinder<T> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public PrefabAssetRuntimeKeyBinder<T> UnderSceneRoot()
        {
            under = null;
            context.Update(this);
            return this;
        }

        public PrefabAssetRuntimeKeyBinder<T> Under(Transform targetTransform)
            => Under(targetTransform, worldPositionStays: true);

        public PrefabAssetRuntimeKeyBinder<T> Under(Transform targetTransform, bool worldPositionStays)
        {
            under = targetTransform;
            this.worldPositionStays = worldPositionStays;
            context.Update(this);
            return this;
        }

        public PrefabAssetRuntimeKeyBinder<T> AsTransient()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Transient;
            context.Update(clone);
            return clone;
        }

        public PrefabAssetRuntimeKeyBinder<T> AsCached()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Cached;
            context.Update(clone);
            return clone;
        }

        public PrefabAssetRuntimeKeyBinder<T> AsSingleton()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Singleton;
            context.Update(clone);
            return clone;
        }

        public FactoryBinder<T, Factory<T>> AsFactory()
            => new(context, AsTransient());

        public FactoryBinder<T, TFactory> AsFactory<TFactory>()
            where TFactory : IFactory<T>
        {
            // if TFactory is Factory<T>, provide resolver to factory
            if (!typeof(Factory<T>).IsAssignableFrom(typeof(TFactory)))
                throw new Exception("PrefabAssetReferenceBinder can only be used with Factory<T>");

            return new FactoryBinder<T, TFactory>(context, AsTransient());
        }
    }
}
#endif