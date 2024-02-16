using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Doinject
{
    public class InstanceBag : IAsyncDisposable
    {
        private ConcurrentDictionary<TargetTypeInfo, ConcurrentObjectBag> InstanceMap { get; } = new();
        internal IReadOnlyDictionary<TargetTypeInfo, ConcurrentObjectBag> ReadOnlyInstanceMap => InstanceMap;

        public bool HasType(TargetTypeInfo targetType)
        {
            return InstanceMap.ContainsKey(targetType);
        }

        public bool Any(TargetTypeInfo targetType)
        {
            if (!HasType(targetType)) return false;
            return InstanceMap[targetType].Any();
        }

        public IEnumerable<object> OfType(TargetTypeInfo targetType)
        {
            return InstanceMap[targetType];
        }

        public void Add(TargetTypeInfo targetType, object instance)
        {
            if (!InstanceMap.ContainsKey(targetType))
            {
                if (!InstanceMap.TryAdd(targetType, new ConcurrentObjectBag()))
                    throw new Exception($"Failed to create InstanceBag for {targetType}");
            }
            InstanceMap[targetType].Add(instance);
        }

        public async Task RemoveAll(TargetTypeInfo targetType)
        {
            if (InstanceMap.TryRemove(targetType, out var toRemove))
            {
                await Task.WhenAll(toRemove.Select(x => Disposer.Dispose(x).AsTask()));

                if (targetType.IsMonoBehaviour)
                {
                    foreach (var x in toRemove.Cast<MonoBehaviour>())
                        if (x) Object.Destroy(x.gameObject);
                }
                else if (targetType.IsGameObject)
                {
                    foreach (var x in toRemove.Cast<GameObject>())
                        if (x) Object.Destroy(x);
                }
                toRemove.Clear();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(InstanceMap.Keys.ToArray().Select(RemoveAll));
            InstanceMap.Clear();
        }
    }
}