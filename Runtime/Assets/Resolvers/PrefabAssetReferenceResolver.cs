#if USE_DI_ASSETS
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Doinject.Assets
{
    public class PrefabAssetReferenceResolver<T> : AbstractInternalResolver<T>, ICacheStrategy
    {
        private PrefabAssetReference PrefabAssetReference { get; }
        private Transform Under { get; }
        private bool WorldPositionStays { get; }
        private GameObject PrefabInstance { get; set; }
        private PrefabResolver<T> PrefabResolver { get; set; }
        private AssetReferenceResolver<GameObject> AssetReferenceResolver { get; set; }
        private object[] Args { get; }
        private TaskQueue TaskQueue { get; } = new();

        public CacheStrategy CacheStrategy { get; }
        public override string ShortName => "PrefabAssetRef";
        public override string StrategyName => CacheStrategy.ToString();
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

        public PrefabAssetReferenceResolver(PrefabAssetReference prefabAssetReference,
            object[] args,
            Transform under,
            bool worldPositionStays,
            CacheStrategy cacheStrategy,
            InstanceBag instanceBag)
            : base(instanceBag)
        {
            PrefabAssetReference = prefabAssetReference;
            Args = args;
            Under = under;
            WorldPositionStays = worldPositionStays;
            CacheStrategy = cacheStrategy;
            TaskQueue.Start();
        }

        public override async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            T instance = default;
            await TaskQueue.EnqueueAsync(async ct =>
            {
                instance = await ResolveInternal(container, args, ct);
            });
            return instance;
        }

        private async Task<T> ResolveInternal(IReadOnlyDIContainer container, object[] args, CancellationToken ct)
        {
            if (CacheStrategy is CacheStrategy.Singleton or CacheStrategy.Cached)
            {
                if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                    return (T)InstanceBag.OfType(TargetType).First();
            }

            var instance = await LoadAsync(container, args);

            if (ct.IsCancellationRequested)
            {
                if (instance is GameObject gameObject)
                    Object.Destroy(gameObject);
                instance = default;
            }

            return instance;
        }

        private async ValueTask<T> LoadAsync(IReadOnlyDIContainer container, object[] args)
        {
            if (AssetReferenceResolver is null)
            {
                AssetReferenceResolver = new AssetReferenceResolver<GameObject>(PrefabAssetReference);
                PrefabInstance = await AssetReferenceResolver.ResolveAsync(container);
            }

            if (PrefabResolver is null)
            {
                PrefabResolver = new PrefabResolver<T>(PrefabInstance, Args ?? args, Under, WorldPositionStays, CacheStrategy, InstanceBag);
            }

            var instance = await PrefabResolver.ResolveAsync(container);
            return instance;
        }

        public async Task TryCacheAsync(DIContainer container)
        {
            if (CacheStrategy == CacheStrategy.Singleton)
                await ResolveAsync(container);
        }

        public override async ValueTask DisposeAsync()
        {
            TaskQueue.Dispose();
            if (AssetReferenceResolver != null)
            {
                await AssetReferenceResolver.DisposeAsync();
                AssetReferenceResolver = null;
            }

            if (PrefabResolver != null)
            {
                await PrefabResolver.DisposeAsync();
                PrefabResolver = null;
            }
            PrefabInstance = null;
        }
    }
}
#endif