using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mew.Core.Extensions;
using Mew.Core.TaskHelpers;
using Mew.Core.UnityObjectHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Doinject
{
    public class DIContainer : IReadOnlyDIContainer, IAsyncDisposable
    {
        private static Dictionary<Type, TargetMethodsInfo> MethodInfoMap { get; } = new();
        private static Dictionary<Type, TargetPropertiesInfo> PropertyInfoMap { get; } = new();
        private static Dictionary<Type, TargetFieldsInfo> FieldInfoMap { get; } = new();

        private IReadOnlyDIContainer Parent { get; set; }
        private Scene Scene { get; set; }
        internal Dictionary<TargetTypeInfo, BinderContext> BinderMap { get; } = new();
        private Dictionary<TargetTypeInfo, IInternalResolver> Resolvers { get; } = new();
        private InstanceBag ResolvedInstanceBag { get; } = new();
        private InstanceBag InstanceBag { get; } = new();
        private CancellationTokenSource CancellationTokenSource { get; } = new();
        private ParallelScope InjectionProcessingScope { get; } = new();
        private ParallelScope AfterInjectionProcessingScope { get; } = new();
        internal ParameterBuilder ParameterBuilder { get; }
        private ConstructorInjector ConstructorInjector { get; }
        private MethodInjector MethodInjector { get; }
        private PropertyInjector PropertyInjector { get; }
        private FieldInjector FieldInjector { get; }

        public IReadOnlyDictionary<TargetTypeInfo, IInternalResolver> ReadOnlyBindings => Resolvers;
        internal IReadOnlyDictionary<TargetTypeInfo, ConcurrentObjectBag> ReadOnlyInstanceMap => ResolvedInstanceBag.ReadOnlyInstanceMap;
        public bool InjectProcessing => InjectionProcessingScope.Processing;
        public bool AfterInjectProcessing => AfterInjectionProcessingScope.Processing;



        public DIContainer(IReadOnlyDIContainer parent = null, Scene scene = default)
        {
            Parent = parent;
            Scene = scene;
            ParameterBuilder = new ParameterBuilder(this);
            ConstructorInjector = new ConstructorInjector(this);
            MethodInjector = new MethodInjector(this);
            PropertyInjector = new PropertyInjector(this);
            FieldInjector = new FieldInjector(this);
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

        internal async ValueTask GenerateResolvers()
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

            foreach (var newResolver in newResolvers)
                await TryCache(newResolver);
        }

        private async ValueTask TryCache(IInternalResolver resolver)
        {
            if (resolver is not ICacheStrategy cacheableResolver) return;
            try { await cacheableResolver.TryCacheAsync(this); }
            catch (Exception e) { throw new FailedToCacheException(resolver, e); }
        }

        public ValueTask<T> InstantiateAsync<T>()
            => InstantiateAsync<T>(Array.Empty<object>());

        public ValueTask<T> InstantiateAsync<T>(object[] args)
            => InstantiateAsync<T>(args, Array.Empty<ScopedInstance>());

        public async ValueTask<T> InstantiateAsync<T>(object[] args, ScopedInstance[] scopedInstances)
            => (T)await InstantiateInternalAsync(typeof(T), args, scopedInstances);

        public ValueTask<object> InstantiateAsync(Type instanceType, object[] args)
            => InstantiateInternalAsync(instanceType, args, Array.Empty<ScopedInstance>());

        private async ValueTask<object> InstantiateInternalAsync(Type targetType, object[] args, ScopedInstance[] scopedInstances)
        {
            InjectionProcessingScope.Begin();
            AfterInjectionProcessingScope.Begin();

            var target = await ConstructorInjector.DoInject(targetType, args, scopedInstances);
            var targetMethodsInfo = GetTargetMethodInfo(targetType);
            var targetPropertiesInfo = GetTargetPropertyInfo(targetType);
            var targetFieldsInfo = GetTargetFieldInfo(targetType);

            try
            {
                await MethodInjector.DoInject(target, targetMethodsInfo, args, scopedInstances);
                await PropertyInjector.DoInject(target, targetPropertiesInfo);
                await FieldInjector.DoInject(target, targetFieldsInfo);
            }
            catch (Exception e)
            {
                InjectionProcessingScope.End();
                AfterInjectionProcessingScope.End();
                throw new FailedToInjectException(target, e);
            }

            InjectionProcessingScope.End();
            await InvokePostInjectCallback(target, targetMethodsInfo);
            AfterInjectionProcessingScope.End();
            InvokeOnInjectedCallback(target, targetMethodsInfo).Forget();

            return target;
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
            var (instance, go) = prefabInstance switch
            {
                GameObject gameObject
                    => (gameObject.GetComponent(targetType), gameObject),
                Component component
                    => (component.GetComponent(targetType), component.gameObject),
                _ => (null, null)
            };

            if (Scene.IsValid())
                SceneManager.MoveGameObjectToScene(go, Scene);

            if (go.GetComponent(typeof(IGameObjectContextRoot)))
                return instance;

            foreach (var component in go.GetComponentsInChildren(typeof(IInjectableComponent)))
                await InjectIntoAsync(component, args);

            return instance;
        }

        public ValueTask InjectIntoAsync<T>(T target)
            => InjectIntoAsync(target, Array.Empty<object>());

        public async ValueTask InjectIntoAsync<T>(T target, object[] args)
            => await InjectIntoInternalAsync(target, args, scopedInstances: Array.Empty<ScopedInstance>());

        private async ValueTask InjectIntoInternalAsync<T>(T target, object[] args, ScopedInstance[] scopedInstances)
        {
            InjectionProcessingScope.Begin();
            AfterInjectionProcessingScope.Begin();

            var targetType = target.GetType();
            var targetMethodsInfo = GetTargetMethodInfo(targetType);
            var targetPropertiesInfo = GetTargetPropertyInfo(targetType);
            var targetFieldsInfo = GetTargetFieldInfo(targetType);

            try
            {
                await MethodInjector.DoInject(target, targetMethodsInfo, args, scopedInstances);
                await PropertyInjector.DoInject(target, targetPropertiesInfo);
                await FieldInjector.DoInject(target, targetFieldsInfo);
            }
            catch (Exception e)
            {
                InjectionProcessingScope.End();
                AfterInjectionProcessingScope.End();
                throw new FailedToInjectException(target, e);
            }

            InjectionProcessingScope.End();
            await InvokePostInjectCallback(target, targetMethodsInfo);
            AfterInjectionProcessingScope.End();
            InvokeOnInjectedCallback(target, targetMethodsInfo).Forget();
        }

        private async ValueTask InvokePostInjectCallback(object target, TargetMethodsInfo methods)
        {
            foreach (var methodInfo in methods.PostInjectMethods)
                await InvokeCallback(target, methodInfo, InjectionProcessingScope);
        }

        private async ValueTask InvokeOnInjectedCallback(object target, TargetMethodsInfo methods)
        {
            await TaskHelper.NextFrame();
            foreach (var methodInfo in methods.OnInjectedMethods)
                await InvokeCallback(target, methodInfo, InjectionProcessingScope);
        }

        private async ValueTask InvokeCallback<T>(T target, MethodInfo callback, ParallelScope completionSource)
        {
            if (callback is null) return;

            await InjectionProcessingScope.Wait();

            if (CancellationTokenSource.IsCancellationRequested) return;

            if (callback.IsTask()) await (Task)callback.Invoke(target, Array.Empty<object>());
            else if (callback.IsValueTask()) await (ValueTask)callback.Invoke(target, Array.Empty<object>());
#if USE_UNITASK
            else if (callback.IsUniTask()) await (UniTask)callback.Invoke(target, Array.Empty<object>());
#endif
            else callback.Invoke(target, Array.Empty<object>());
        }

        internal void MarkInjected(Type resolverType)
        {
            if (!Resolvers.TryGetValue(new TargetTypeInfo(resolverType), out var resolver)) return;

            // If target in InstanceResolver try to set Injected flag
            if (resolver is IInstanceResolver instanceResolver)
                instanceResolver.Injected = true;
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

            if (Parent is DIContainer raw)
            {
                if (raw.InjectProcessing)
                    throw new Exception("Parent container is processing injection. Use OnInjected callback.");
                return await Parent.ResolveAsync<T>();
            }

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

            if (Parent is DIContainer raw)
            {
                if (raw.InjectProcessing)
                    throw new Exception("Parent container is processing injection. Use OnInjected callback.");
                return await Parent.ResolveAsync(targetType);
            }

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

        public void PushInstance(object instance)
        {
            InstanceBag.Add(new TargetTypeInfo(instance.GetType()), instance);
        }

        private TargetMethodsInfo GetTargetMethodInfo(Type targetType)
        {
            if (MethodInfoMap.TryGetValue(targetType, out var targetMethodsInfo))
                return targetMethodsInfo;

            targetMethodsInfo = new TargetMethodsInfo(targetType);
            MethodInfoMap[targetType] = targetMethodsInfo;
            return targetMethodsInfo;
        }

        private TargetPropertiesInfo GetTargetPropertyInfo(Type targetType)
        {
            if (PropertyInfoMap.TryGetValue(targetType, out var targetPropertiesInfo))
                return targetPropertiesInfo;

            targetPropertiesInfo = new TargetPropertiesInfo(targetType);
            PropertyInfoMap[targetType] = targetPropertiesInfo;
            return targetPropertiesInfo;
        }

        private TargetFieldsInfo GetTargetFieldInfo(Type targetType)
        {
            if (FieldInfoMap.TryGetValue(targetType, out var targetFieldsInfo))
                return targetFieldsInfo;

            targetFieldsInfo = new TargetFieldsInfo(targetType);
            FieldInfoMap[targetType] = targetFieldsInfo;
            return targetFieldsInfo;
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