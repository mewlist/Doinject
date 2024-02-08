#if USE_DI_ASSETS
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Doinject.Assets
{
    public struct AssetRuntimeKeyBinder<T> : IBinder
        where T : Object
    {
        private readonly BinderContext context;
        private object RuntimeKey { get; }

        public AssetRuntimeKeyBinder(BinderContext context, object runtimeKey)
        {
            this.context = context;
            RuntimeKey = runtimeKey;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new AddressableAssetResolver<T>(RuntimeKey);
        }
    }
}
#endif