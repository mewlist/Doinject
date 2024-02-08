#if USE_DI_ASSETS
using System;
using Mew.Core.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Doinject.Assets
{
    public static class DIContainerExtension
    {
        public static PrefabAssetRuntimeKeyBinder<T> BindPrefabAssetReference<T>(this DIContainer container, PrefabAssetReference prefabAssetReference)
            where T: Component
        {
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            ValidatePrefabAssetReference<T>(prefabAssetReference);
            var binder = new PrefabAssetRuntimeKeyBinder<T>(binderContext, prefabAssetReference);
            container.BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public static PrefabAssetRuntimeKeyBinder<T> BindPrefabAssetRuntimeKey<T>(this DIContainer container, object runtimeKey)
            where T: Component
        {
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            var binder = new PrefabAssetRuntimeKeyBinder<T>(binderContext, runtimeKey);
            container.BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public static AssetRuntimeKeyBinder<T> BindAssetReference<T>(this DIContainer container, AssetReference assetReference)
            where T: Object
        {
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            ValidateAssetReference<T>(assetReference);
            var binder = new AssetRuntimeKeyBinder<T>(binderContext, assetReference);
            container.BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public static AssetRuntimeKeyBinder<T> BindAssetRuntimeKey<T>(this DIContainer container, object runTimeKey)
            where T: Object
        {
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            var binder = new AssetRuntimeKeyBinder<T>(binderContext, runTimeKey);
            container.BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        private static void ValidatePrefabAssetReference<T>(PrefabAssetReference prefabAssetReference)
            where T: Component
        {
#if UNITY_EDITOR
            var go = prefabAssetReference.editorAsset as GameObject;
            var component = go.GetComponent<T>();
            if (!component)
            {
                var message = $"prefab does not have {typeof(T)}";
                Debug.LogError(message, prefabAssetReference.editorAsset);
                throw new Exception(message);
            }
#endif
        }

        private static void ValidateAssetReference<T>(AssetReference assetReference)
        {
#if UNITY_EDITOR
            if (assetReference.editorAsset is not T)
            {
                var message = $"asset reference is not subclass of {typeof(T)}";
                Debug.LogError(message, assetReference.editorAsset);
                throw new Exception(message);
            }
#endif
        }
    }
}
#endif