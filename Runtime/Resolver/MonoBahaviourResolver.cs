using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Doinject
{
    public sealed class MonoBahaviourResolver<T, TInstance> : AbstractInternalResolver<T>, ICacheStrategy
        where TInstance : T
    {
        private TargetTypeInfo InstanceType { get; }
        private object[] Args { get; }
        private GameObject On { get; }
        private Transform Under { get; }
        private bool WorldPositionStays { get; }

        public CacheStrategy CacheStrategy { get; }

        public MonoBahaviourResolver(object[] args, CacheStrategy cacheStrategy, InstanceBag instanceBag, GameObject on)
            : base(instanceBag)
        {
            InstanceType = new TargetTypeInfo(typeof(TInstance));
            Assert.IsTrue(InstanceType.Type == typeof(MonoBehaviour) || InstanceType.Type.IsSubclassOf(typeof(MonoBehaviour)));
            Args = args;
            CacheStrategy = cacheStrategy;
            On = on;
        }


        public MonoBahaviourResolver(object[] args, CacheStrategy cacheStrategy, InstanceBag instanceBag, Transform under, bool worldPositionStays)
            : base(instanceBag)
        {
            InstanceType = new TargetTypeInfo(typeof(TInstance));
            Assert.IsTrue(InstanceType.Type == typeof(MonoBehaviour) || InstanceType.Type.IsSubclassOf(typeof(MonoBehaviour)));
            Args = args;
            CacheStrategy = cacheStrategy;
            On = null;
            Under = under;
            WorldPositionStays = worldPositionStays;
        }

        public override async ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null)
        {
            switch (CacheStrategy)
            {
                case CacheStrategy.Singleton: case CacheStrategy.Cached:
                    if (InstanceBag.HasType(TargetType) && InstanceBag.Any(TargetType))
                        return (T)InstanceBag.OfType(TargetType).First();
                    break;
            }

            var instance = await Instantiate(container, args);
            if (CacheStrategy != CacheStrategy.Transient)
                InstanceBag.Add(TargetType, instance);
            return instance;
        }

        private async ValueTask<T> Instantiate(IReadOnlyDIContainer container, object[] args)
        {
            if (Under)
                return await container.InstantiateMonoBehaviourAsync<TInstance>(Under, WorldPositionStays, Args);

            return await container.InstantiateMonoBehaviourAsync<TInstance>(On, Args ?? args);
        }

        public async Task TryCacheAsync(DIContainer container)
        {
            if (CacheStrategy == CacheStrategy.Singleton)
                await ResolveAsync(container);
        }

        public override async ValueTask DisposeAsync()
        {
            await InstanceBag.RemoveAll(TargetType);
        }
    }
}