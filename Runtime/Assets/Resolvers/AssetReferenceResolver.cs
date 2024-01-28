#if USE_DI_ASSETS
using System.Threading.Tasks;
using Mew.Core.Assets;
using Mew.Core.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Doinject.Assets
{
    public sealed class AssetReferenceResolver<T> : IInternalResolver<T>
        where T: Object
    {
        private TargetTypeInfo TargetType { get; }
        private AssetReference AssetReference { get; }
        private AssetLoader AssetLoader { get; } = new();
        private T Instance { get; set; }
        private TaskQueue TaskQueue { get; } = new();

        public string Name => $"{GetType().Name}<{typeof(T).Name}>";

        public AssetReferenceResolver(AssetReference assetReference)
        {
            TargetType = new TargetTypeInfo(typeof(T));
            AssetReference = assetReference;
            TaskQueue.Start();
        }

        public async ValueTask<object> ResolveAsObjectAsync(DIContainer container)
        {
            return await ResolveAsync(container);
        }

        public async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            if (Instance is not null) return Instance;
            Instance ??= await LoadAsync();
            return Instance;
        }

        private async ValueTask<T> LoadAsync()
        {
            T instance = default;
            await TaskQueue.EnqueueAsync(async ct =>
            {
                if (Instance is not null) return;
                instance = await AssetLoader.LoadAsync<T>(AssetReference, ct);
            }).OnException(Debug.LogException);
            return instance;
        }

        public ValueTask DisposeAsync()
        {
            TaskQueue.Dispose();
            AssetLoader.Dispose();
            Instance = default;
            return default;
        }
    }
}
#endif