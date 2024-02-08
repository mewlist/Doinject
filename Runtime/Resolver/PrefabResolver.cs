using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Doinject
{
    public sealed class PrefabResolver<T> : AbstractInternalResolver<T>, ICacheStrategy
    {
        private AwaitableCompletionSource CachingCompletionSource { get; set; }
        private Object Prefab { get; }
        private object[] Args { get; }
        private Transform Under { get; }
        private bool WorldPositionStays { get; }

        public CacheStrategy CacheStrategy { get; }
        public override string ShortName => "Prefab";
        public override string StrategyName => CacheStrategy.ToString();
        public override int InstanceCount => InstanceBag.HasType(TargetType) ? InstanceBag.OfType(TargetType).Count() : 0;

        public PrefabResolver(Object prefab,
            object[] args,
            Transform under,
            bool worldPositionStays,
            CacheStrategy cacheStrategy,
            InstanceBag instanceBag)
            : base(instanceBag)
        {
            ValidatePrefab(prefab);
            Prefab = prefab;
            Args = args;
            Under = under;
            WorldPositionStays = worldPositionStays;
            CacheStrategy = cacheStrategy;
        }


        public override async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            switch (CacheStrategy)
            {
                case CacheStrategy.Singleton: case CacheStrategy.Cached:
                    if (CachingCompletionSource != null) await CachingCompletionSource.Awaitable;
                    if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                        return (T)InstanceBag.OfType(TargetType).First();
                    CachingCompletionSource = new AwaitableCompletionSource();
                    break;
            }

            var instance = await Instantiate(container, args);
            if (CacheStrategy != CacheStrategy.Transient)
                InstanceBag.Add(TargetType, instance);
            CachingCompletionSource?.TrySetResult();
            CachingCompletionSource = null;
            return instance;
        }

        private async ValueTask<T> Instantiate(IReadOnlyDIContainer container, object[] args)
        {
            var instance = (T)await container.InstantiatePrefabAsync(TargetType.Type, Args ?? args, Prefab);

            if (Under && instance is Component component)
                component.transform.SetParent(Under, WorldPositionStays);
            else if (Under && instance is GameObject gameObject)
                gameObject.transform.SetParent(Under, WorldPositionStays);

            return instance;
        }

        public async Task TryCacheAsync(DIContainer container)
        {
            if (CacheStrategy == CacheStrategy.Singleton)
                await ResolveAsync(container);
        }

        private void ValidatePrefab(Object prefab)
        {
            if (prefab is MonoBehaviour monoBehaviour)
                Assert.IsTrue(monoBehaviour.GetComponent(TargetType.Type));
            else if (prefab is GameObject go)
                Assert.IsTrue(go.GetComponent(TargetType.Type));
            else
                throw new Exception("Prefab must be a GameObject or a MonoBehaviour.");

        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}