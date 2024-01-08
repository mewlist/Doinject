#if USE_DI_ASSETS
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Doinject.Assets
{
    public struct AssetReferenceBinder<T> : IBinder
        where T: Object
    {
        private readonly BinderContext context;
        private AssetReference AssetReference { get; }

        public AssetReferenceBinder(BinderContext context, AssetReference assetReference)
        {
            this.context = context;
            AssetReference = assetReference;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new AssetReferenceResolver<T>(AssetReference);
        }
    }
}
#endif