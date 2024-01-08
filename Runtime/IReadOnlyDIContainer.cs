using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Doinject
{
    public interface IReadOnlyDIContainer
    {
        ValueTask InjectIntoAsync<T>(T target);
        ValueTask InjectIntoAsync<T>(T target, object[] args);
        ValueTask<T> InstantiateAsync<T>();
        ValueTask<T> InstantiateAsync<T>(object[] args);
        ValueTask<T> InstantiateAsync<T>(object[] args, ScopedInstance[] scopedInstances);
        ValueTask<object> InstantiateAsync(Type instanceType, object[] args);
        ValueTask<object> InstantiatePrefabAsync(Type targetType, object[] args, Object prefab);
        ValueTask<T> InstantiateMonoBehaviourAsync<T>(GameObject on, object[] args);
        ValueTask<object> InstantiateMonoBehaviourAsync(Type instanceType, GameObject on, object[] args);
        ValueTask<T> InstantiateMonoBehaviourAsync<T>(Transform under, bool worldPositionStays, object[] args);
        ValueTask<object> InstantiateMonoBehaviourAsync(Type targetType, Transform under, bool worldPositionStays, object[] args);
        ValueTask<T> ResolveAsync<T>();
        ValueTask<object> ResolveAsync(Type targetType);
        bool HasBinding<T>();
        bool HasBinding(Type targetType);
    }
}