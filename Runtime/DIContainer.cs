using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.Extensions;
using Mew.Core.TaskHelpers;
using Mew.Core.UnityObjectHelpers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Doinject
{
    public class DIContainer : IReadOnlyDIContainer, IAsyncDisposable
    {
        private IReadOnlyDIContainer Parent { get; set; }
        private Scene Scene { get; set; }
        internal Dictionary<TargetTypeInfo, BinderContext> BinderMap { get; } = new();
        private Dictionary<TargetTypeInfo, IInternalResolver> Resolvers { get; } = new();
        private InstanceBag ResolvedInstanceBag { get; } = new();
        private InstanceBag InstanceBag { get; } = new();
        private CancellationTokenSource CancellationTokenSource { get; } = new();
        private AwaitableCompletionSource InjectionProcessingCompletionSource { get; } = new();
        private int InjectionProcessingCount { get; set; }


        public IReadOnlyDictionary<TargetTypeInfo, IInternalResolver> ReadOnlyBindings => Resolvers;
        public IReadOnlyDictionary<TargetTypeInfo, ConcurrentObjectBag> ReadOnlyInstanceMap => ResolvedInstanceBag.ReadOnlyInstanceMap;
        public bool InjectionProcessing => InjectionProcessingCount > 0;



        public DIContainer(IReadOnlyDIContainer parent = null, Scene scene = default)
        {
            Parent = parent;
            Scene = scene;
            BindFromInstance<IReadOnlyDIContainer, DIContainer>(this);
        }

        public FixedBinder BindTransient<T>()
            => BindTransient<T, T>();

        public FixedBinder BindTransient<T, TInstance>() where TInstance : T
            => Bind<T, TInstance>().AsTransient();

        public FixedBinder BindCached<T>()
            => Bind<T,T>().AsCached();

        public void BindSingleton<T>()
            => Bind<T,T>().AsSingleton();

        public TypeBinder<T, T> Bind<T>()
            => Bind<T, T>();

        public TypeBinder<T, TInstance> Bind<T, TInstance>()
            where TInstance : T
        {
            var targetType = typeof(T);
            ValidateTargetType(targetType);

            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            var binder = new TypeBinder<T, TInstance>(binderContext);
            BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public InstanceBinder<T, T> BindFromInstance<T>(T instance)
        {
            return BindFromInstance<T, T>(instance);
        }

        public InstanceBinder<T, TInstance> BindFromInstance<T, TInstance>(TInstance instance)
            where TInstance : T
        {
            var targetType = typeof(T);
            var instanceType = typeof(TInstance);

            ValidateTargetType(targetType);

            var targetTypeInfo = new TargetTypeInfo(targetType);

            var binderContext = new BinderContext();
            var binder = new InstanceBinder<T, TInstance>(binderContext, instance);
            BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public PrefabBinder<T> BindPrefab<T>(Object prefab)
        {
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);
            var binderContext = new BinderContext();
            var binder = new PrefabBinder<T>(binderContext, prefab);
            BinderMap[targetTypeInfo] = binderContext;
            return binder;
        }

        public void Unbind<T>()
        {
            Unbind(typeof(T));
        }

        public void Unbind(Type targetType)
        {
            var targetTypeInfo = new TargetTypeInfo(targetType);
            if (!ReadOnlyBindings.ContainsKey(targetTypeInfo) && !BinderMap.ContainsKey(targetTypeInfo))
                throw new KeyNotFoundException($"No binding for type [{targetType}]");

            if (BinderMap.ContainsKey(targetTypeInfo))
            {
                BinderMap.Remove(targetTypeInfo);
            }

            if (ReadOnlyBindings.ContainsKey(targetTypeInfo))
            {
                var resolver = Resolvers[targetTypeInfo];
                Resolvers.Remove(targetTypeInfo);
                resolver.DisposeAsync().Forget();
            }
        }

        public async Task GenerateResolvers()
        {
            if (!BinderMap.Any()) return;
            var toConvert = new Dictionary<TargetTypeInfo, BinderContext>(BinderMap);
            BinderMap.Clear();
            var newResolvers = new List<IInternalResolver>();
            foreach (var (targetType, binderContext) in toConvert)
            {
                var resolver = binderContext.ToResolver(ResolvedInstanceBag);
                if (resolver is IFactoryResolver factoryResolver)
                {
                    var factoryInterfaces = factoryResolver.FactoryType.FindFactoryInterfaces();
                    Resolvers[factoryResolver.FactoryType] = resolver;
                    Resolvers[factoryInterfaces] = resolver;
                }
                else
                    Resolvers[targetType] = resolver;

                newResolvers.Add(resolver);
            }

            // try cache
            foreach (var newResolver in newResolvers)
            {
                if (newResolver is not ICacheStrategy cacheableResolver) continue;
                try
                {
                    await cacheableResolver.TryCacheAsync(this);
                }
                catch (Exception e)
                {
                    throw new FailedToCacheException(newResolver, e);
                }
            }
        }

        public ValueTask<T> InstantiateAsync<T>()
            => InstantiateAsync<T>(Array.Empty<object>());

        public ValueTask<T> InstantiateAsync<T>(object[] args)
            => InstantiateAsync<T>(args, Array.Empty<ScopedInstance>());

        public async ValueTask<T> InstantiateAsync<T>(object[] args, ScopedInstance[] scopedInstances)
            => (T)await InstantiateAsync(typeof(T), args, scopedInstances);

        public ValueTask<object> InstantiateAsync(Type instanceType, object[] args)
            => InstantiateAsync(instanceType, args, Array.Empty<ScopedInstance>());

        public async ValueTask<object> InstantiateAsync(Type targetType, object[] args, ScopedInstance[] scopedInstances)
        {
            if (targetType.IsInterface)
                throw new Exception($"Instance type [{targetType.Name}] is interface".ToExceptionMessage());

            var constructors = targetType.GetConstructors();
            var constructor = constructors.First();
            var parameters = constructor.GetParameters();
            object instance;
            if (parameters.Any())
            {
                var buildParameters = await ResolveParameters(targetType, parameters, args ?? Array.Empty<object>(), scopedInstances);
                try
                {
                    instance = constructor.Invoke(buildParameters);
                }
                catch (Exception e)
                {
                    if (e.InnerException is InvalidOperationException)
                        throw e.InnerException;
                    throw;
                }
            }
            else
            {
                instance = Activator.CreateInstance(targetType);
            }
            await InjectIntoAsync(instance, args, scopedInstances);
            return instance;
        }


        public async ValueTask<T> InstantiateMonoBehaviourAsync<T>(GameObject on, object[] args)
        {
            return (T)await InstantiateMonoBehaviourAsync(typeof(T), on, args);
        }

        public async ValueTask<object> InstantiateMonoBehaviourAsync(Type targetType, GameObject on, object[] args)
        {
            var instance = on
                ? UnityObjectHelper.InstantiateComponentOnGameObject(targetType, on)
                : UnityObjectHelper.InstantiateComponentInSceneRoot(targetType, Scene);
            await InjectIntoAsync(instance, args);
            return instance;
        }

        public async ValueTask<T> InstantiateMonoBehaviourAsync<T>(Transform under, bool worldPositionStays, object[] args)
        {
            return (T)await InstantiateMonoBehaviourAsync(typeof(T), under, worldPositionStays, args);
        }

        public async ValueTask<object> InstantiateMonoBehaviourAsync(Type targetType, Transform under, bool worldPositionStays, object[] args)
        {
            var instance = under
                ? UnityObjectHelper.InstantiateComponentUnderTransform(targetType, under, worldPositionStays)
                : UnityObjectHelper.InstantiateComponentInSceneRoot(targetType, Scene);
            await InjectIntoAsync(instance, args);
            return instance;
        }

        public async ValueTask<object> InstantiatePrefabAsync(Type targetType, object[] args, Object prefab)
        {
            var prefabInstance = Object.Instantiate(prefab);
            var instance = prefabInstance switch
            {
                GameObject gameObject => gameObject.GetComponent(targetType),
                Component component => component.GetComponent(targetType),
                _ => null
            };
            var go = prefabInstance switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => null
            };

            if (Scene.IsValid())
            {
                using var instanceIds = new NativeArray<int>( new [] { go.GetInstanceID() }, Allocator.Temp);
                SceneManager.MoveGameObjectsToScene(instanceIds, Scene);
            }

            if (go.GetComponent(typeof(IGameObjectContextRoot)))
                return instance;

            foreach (var component in go.GetComponentsInChildren(typeof(IInjectableComponent)))
                await InjectIntoAsync(component, args: args);

            return instance;
        }

        public ValueTask InjectIntoAsync<T>(T target)
            => InjectIntoAsync(target, Array.Empty<object>());

        public ValueTask InjectIntoAsync<T>(T target, object[] args)
            => InjectIntoAsync(target, args, Array.Empty<ScopedInstance>());

        private async ValueTask InjectIntoAsync<T>(T target, object[] args, ScopedInstance[] scopedInstances)
        {
            if (InjectionProcessingCount == 0)
                InjectionProcessingCompletionSource.Reset();
            InjectionProcessingCount++;

            var targetType = target.GetType();
            var resolverType = targetType;

            // If target in InstanceResolver try to set Injected flag
            if (targetType == typeof(IInjectableComponent))
                resolverType = target.GetType();

            if (Resolvers.TryGetValue(new TargetTypeInfo(resolverType), out var resolver))
            {
                if (resolver is IInstanceResolver instanceResolver)
                    instanceResolver.Injected = true;
            }

            var methods = targetType.GetMethods();
            foreach (var methodInfo in methods.Reverse())
            {
                try
                {
                    var attr = methodInfo.GetCustomAttributes(typeof(InjectAttribute), true);
                    if (!attr.Any()) continue;
                    await DoInject(target, methodInfo, args, scopedInstances);
                }
                catch (Exception e)
                {
                    InjectionProcessingCount--;
                    if (!InjectionProcessing)
                    {
                        InjectionProcessingCompletionSource.TrySetResult();
                    }
                    throw new FailedToInjectException(target, e);
                }
            }

            InjectionProcessingCount--;
            if (!InjectionProcessing)
                InjectionProcessingCompletionSource.TrySetResult();

            var onInjectedCallback = methods.FirstOrDefault(x => x.Name == "OnInjected" && x.GetParameters().Length == 0);
            InvokeCallback(
                target,
                onInjectedCallback,
                completionSource: InjectionProcessing ? InjectionProcessingCompletionSource : null)
                .Forget();
        }

        private async ValueTask DoInject<T>(T target, MethodInfo methodInfo, object[] args,
            ScopedInstance[] scopedInstances)
        {
            var parameters = await ResolveParameters(target.GetType(), methodInfo.GetParameters(), args, scopedInstances);
            if (methodInfo.ReturnType == typeof(Task))
            {
                var task = (Task)methodInfo.Invoke(target, parameters);
                await task;
            }
            else if (methodInfo.ReturnType == typeof(ValueTask))
            {
                var task = (ValueTask)methodInfo.Invoke(target, parameters);
                await task;
            }
            else
            {
                methodInfo.Invoke(target, parameters);
            }
        }

        private async ValueTask InvokeCallback<T>(T target, MethodInfo callback, AwaitableCompletionSource completionSource)
        {
            if (callback is null) return;

            if (completionSource is not null)
                await completionSource.Awaitable;
            else
                await TaskHelper.NextFrame();

            if (CancellationTokenSource.IsCancellationRequested) return;

            if (callback.ReturnType == typeof(Task))
            {
                var task = (Task)callback.Invoke(target, Array.Empty<object>());
                await task;
            }
            else if (callback.ReturnType == typeof(ValueTask))
            {
                var task = (ValueTask)callback.Invoke(target, Array.Empty<object>());
                await task;
            }
            else
            {
                callback.Invoke(target, Array.Empty<object>());
            }
        }

        private async ValueTask<object[]> ResolveParameters(
            Type targetType,
            IEnumerable<ParameterInfo> parameterInfos,
            object[] args,
            ScopedInstance[] scopedInstances)
        {
            var parameters = new List<object>();
            var argsQueue = new Queue<object>(args ?? Array.Empty<object>());
            foreach (var parameterInfo in parameterInfos)
            {
                try
                {
                    var parameter = await ResolveAsync(parameterInfo.ParameterType);
                    parameters.Add(parameter);
                    continue;
                }
                catch (FailedToResolveException)
                {
                    // ignored
                }

                if (argsQueue.Any() && argsQueue.Peek().GetType() == parameterInfo.ParameterType)
                {
                    parameters.Add(argsQueue.Dequeue());
                    continue;
                }

                var matched = scopedInstances
                    .FirstOrDefault(x => x.TargetType == parameterInfo.ParameterType);

                if (matched.IsValid)
                {
                    parameters.Add(matched.Instance);
                }
                else
                {
                    var optionalAttribute = parameterInfo.GetCustomAttribute(typeof(OptionalAttribute), true);
                    var parameterType = parameterInfo.ParameterType;

                    if (optionalAttribute is null)
                        throw new FailedToResolveParameterException(parameterInfo.ParameterType, targetType);

                    parameters.Add(parameterType.IsValueType
                        ? Activator.CreateInstance(parameterType)
                        : null);
                }
            }

            return parameters.ToArray();
        }

        public async ValueTask<T> ResolveAsync<T>()
        {
            await GenerateResolvers();
            var targetType = typeof(T);
            var targetTypeInfo = new TargetTypeInfo(targetType);

            if (HasBinding(targetType))
            {
                var instance = await (Resolvers[targetTypeInfo]).ResolveAsObjectAsync(this);
                return (T)instance;
            }

            if (Parent is not null)
                return await Parent.ResolveAsync<T>();

            throw new FailedToResolveException(targetType);
        }

        public async ValueTask<object> ResolveAsync(Type targetType)
        {
            await GenerateResolvers();
            var targetTypeInfo = new TargetTypeInfo(targetType);

            if (HasBinding(targetType))
            {
                var instance = await Resolvers[targetTypeInfo].ResolveAsObjectAsync(this);
                return instance;
            }

            if (Parent is not null)
                return await Parent.ResolveAsync(targetType);

            throw new FailedToResolveException(targetType);
        }

        public bool HasBinding<T>()
        {
            return HasBinding(typeof(T));
        }

        public bool HasBinding(Type targetType)
        {
            var targetTypeInfo = new TargetTypeInfo(targetType);
            return BinderMap.ContainsKey(targetTypeInfo) || ReadOnlyBindings.ContainsKey(targetTypeInfo);
        }

        private void ValidateTargetType(Type targetType)
        {
            if (HasBinding(targetType))
                throw new Exception($"Already has binding for type [{targetType}]");
        }

        public void PushInstance(object installer)
        {
            InstanceBag.Add(new TargetTypeInfo(installer.GetType()), installer);
        }

        public async ValueTask DisposeAsync()
        {
            if (CancellationTokenSource.IsCancellationRequested) return;
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();

            await Task.WhenAll(Resolvers.Select(x => x.Value.DisposeAsync().AsTask()));
            Resolvers.Clear();
            await ResolvedInstanceBag.DisposeAsync();
            await InstanceBag.DisposeAsync();
            Parent = null;
            Scene = default;
        }
    }
}