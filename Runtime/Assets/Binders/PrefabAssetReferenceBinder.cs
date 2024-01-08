#if USE_DI_ASSETS
using System;
using Mew.Core.Assets;
using UnityEngine;

namespace Doinject.Assets
{
    public struct PrefabAssetReferenceBinder<T> : IBinder
    {
        private readonly BinderContext context;
        private readonly PrefabAssetReference prefabAssetReference;
        private object[] args;

        private Transform under;
        private bool worldPositionStays;
        private CacheStrategy cacheStrategy;

        public PrefabAssetReferenceBinder(BinderContext context, PrefabAssetReference prefabAssetReference)
        {
            this.context = context;
            this.prefabAssetReference = prefabAssetReference;
            this.args = null;
            this.under = default;
            this.worldPositionStays = true;
            cacheStrategy = CacheStrategy.Cached;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new PrefabAssetReferenceResolver<T>(prefabAssetReference, args, under, worldPositionStays, cacheStrategy, instanceBag);
        }

        public PrefabAssetReferenceBinder<T> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public PrefabAssetReferenceBinder<T> UnderSceneRoot()
        {
            under = null;
            context.Update(this);
            return this;
        }

        public PrefabAssetReferenceBinder<T> Under(Transform targetTransform)
            => Under(targetTransform, worldPositionStays: true);

        public PrefabAssetReferenceBinder<T> Under(Transform targetTransform, bool worldPositionStays)
        {
            under = targetTransform;
            this.worldPositionStays = worldPositionStays;
            context.Update(this);
            return this;
        }

        public PrefabAssetReferenceBinder<T> AsTransient()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Transient;
            context.Update(clone);
            return clone;
        }

        public PrefabAssetReferenceBinder<T> AsCached()
        {
            var clone = this;
            clone.cacheStrategy = CacheStrategy.Cached;
            context.Update(clone);
            return clone;
        }

        public PrefabAssetReferenceBinder<T> AsSingleton()
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