using System;
using System.Reflection;
using UnityEngine;

namespace Doinject
{
    public struct TypeBinder<T, TInstance> : IBinder
        where TInstance : T
    {
        private readonly BinderContext context;
        private object[] args;
        private TargetTypeInfo InstanceTypeInfo => new(typeof(TInstance));

        public TypeBinder(BinderContext context)
        {
            this.context = context;
            this.args = null;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            var typeInfo = new TargetTypeInfo(typeof(TInstance));
            if (typeInfo.IsMonoBehaviour) return UnderSceneRoot().ToResolver(instanceBag);
            else return AsCached().ToResolver(instanceBag);
        }

        public TypeBinder<T, TInstance> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public InstanceBinder<T, T2> FromInstance<T2>(T2 instance)
            where T2 : T
            => new(context, instance);

        public FromResolverBinder<T, TResolver> FromResolver<TResolver>()
            where TResolver : IResolver<T>, new()
            => new FromResolverBinder<T, TResolver>(context).Args(args);

        public TypeBinder<T, T2> To<T2>() where T2 : T
        {
            context.ValidateInstanceType(typeof(T2));
            return new TypeBinder<T, T2>(context);
        }

        public InstantiateMonoBehaviourBinder<T, TInstance> On(GameObject targetGameObject) =>
            new InstantiateMonoBehaviourBinder<T, TInstance>(context, targetGameObject).Args(args);

        public InstantiateMonoBehaviourBinder<T, TInstance> UnderSceneRoot()
            => new InstantiateMonoBehaviourBinder<T, TInstance>(context, null).Args(args);

        public InstantiateMonoBehaviourBinder<T, TInstance> Under(Transform targetTransform)
            => Under(targetTransform, worldPositionStays: true);

        public InstantiateMonoBehaviourBinder<T, TInstance> Under(Transform targetTransform, bool worldPositionStays) =>
            new InstantiateMonoBehaviourBinder<T, TInstance>(context, targetTransform, worldPositionStays).Args(args);

        public FixedBinder AsCached()
            => InstanceTypeInfo.IsMonoBehaviour
                ? new InstantiateMonoBehaviourBinder<T, TInstance>(context,  on: null).Args(args).AsCached()
                : new InstantiateBinder<T, TInstance>(context).Args(args).AsCached();

        public FixedBinder AsTransient()
            => InstanceTypeInfo.IsMonoBehaviour
                ? new InstantiateMonoBehaviourBinder<T, TInstance>(context,  on: null).Args(args).AsTransient()
                : new InstantiateBinder<T, TInstance>(context).Args(args).AsTransient();

        public FixedBinder AsSingleton()
            => InstanceTypeInfo.IsMonoBehaviour
                ? new InstantiateMonoBehaviourBinder<T, TInstance>(context,  on: null).Args(args).AsSingleton()
                : new InstantiateBinder<T, TInstance>(context).Args(args).AsSingleton();

        public FactoryBinder<T, Factory<T>> AsFactory()
            => new(context, AsTransient());

        public FactoryBinder<T, Factory<TArg1, T>> AsFactory<TArg1>()
        {
            if (args is not null)
                throw new Exception($"Remove Args() call before calling {MethodBase.GetCurrentMethod()?.Name}");
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TArg1)))
                throw new Exception($"{typeof(TArg1).Name} is Factory. Call AsCustomFactory<{typeof(TArg1).Name}>() instead of AsFactory<{typeof(TArg1).Name}>()");
            return new(context, AsTransient());
        }

        public FactoryBinder<T, Factory<TArg1, TArg2, T>> AsFactory<TArg1, TArg2>()
        {
            if (args is not null)
                throw new Exception($"Remove Args() call before calling {MethodBase.GetCurrentMethod()?.Name}");
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TArg1)))
                throw new Exception($"{typeof(TArg1).Name} is Factory. Call AsCustomFactory<{typeof(TArg1).Name}>() instead of AsFactory<{typeof(TArg1).Name}>()");
            return new(context, AsTransient());
        }

        public FactoryBinder<T, Factory<TArg1, TArg2, TArg3, T>> AsFactory<TArg1, TArg2, TArg3>()
        {
            if (args is not null)
                throw new Exception($"Remove Args() call before calling {MethodBase.GetCurrentMethod()?.Name}");
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TArg1)))
                throw new Exception($"{typeof(TArg1).Name} is Factory. Call AsCustomFactory<{typeof(TArg1).Name}>() instead of AsFactory<{typeof(TArg1).Name}>()");
            return new(context, AsTransient());
        }

        public FactoryBinder<T, Factory<TArg1, TArg2, TArg3, TArg4, T>> AsFactory<TArg1, TArg2, TArg3, TArg4>()
        {
            if (args is not null)
                throw new Exception($"Remove Args() call before calling {MethodBase.GetCurrentMethod()?.Name}");
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TArg1)))
                throw new Exception($"{typeof(TArg1).Name} is Factory. Call AsCustomFactory<{typeof(TArg1).Name}>() instead of AsFactory<{typeof(TArg1).Name}>()");
            return new(context, AsTransient());
        }

        public FactoryBinder<T, TFactory> AsCustomFactory<TFactory>()
            where TFactory : IFactory
            => new FactoryBinder<T, TFactory>(context, CreateInnerBinder<TFactory>()).Args(args);

        private IBinder CreateInnerBinder<TFactory>() where TFactory : IFactory
        {
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TFactory)))
                return AsTransient();
            if (typeof(TInstance) != typeof(T))
                throw new Exception("Custom factory cannot be used with custom instance type");
            return new ImplicitBinder<T>();
        }
    }
}