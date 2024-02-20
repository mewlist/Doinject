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
        private object PrefabAssetRuntimeKey { get; }
        private Transform Under { get; }
        private bool WorldPositionStays { get; }
        private GameObject PrefabInstance { get; set; }
        private PrefabResolver<T> PrefabResolver { get; set; }
        private AddressableAssetResolver<GameObject> AddressableAssetResolver { get; set; }
        private object[] Args { get; }
        private TaskQueue TaskQueue { get; } = new();

        public CacheStrategy CacheStrategy { get; }
        public override string ShortName => "PrefabAssetRef";
        public override string StrategyName => CacheStrategy.ToString();
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

        public PrefabAssetReferenceResolver(object prefabAssetRuntimeKey,
            object[] args,
            Transform under,
            bool worldPositionStays,
            CacheStrategy cacheStrategy,
            InstanceBag instanceBag)
            : base(instanceBag)
        {
            PrefabAssetRuntimeKey = prefabAssetRuntimeKey;
            Args = args;
            Under = under;
            WorldPositionStays = worldPositionStays;
            CacheStrategy = cacheStrategy;
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
            if (AddressableAssetResolver is null)
            {
                AddressableAssetResolver = new AddressableAssetResolver<GameObject>(PrefabAssetRuntimeKey);
                PrefabInstance = await AddressableAssetResolver.ResolveAsync(container);
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
            if (AddressableAssetResolver != null)
            {
                await AddressableAssetResolver.DisposeAsync();
                AddressableAssetResolver = null;
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