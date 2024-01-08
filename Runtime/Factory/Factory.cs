using System.Threading.Tasks;

namespace Doinject
{
    public abstract class AbstractFactory<T> : IFactory
    {
        protected IResolver<T> Resolver { get; set; }
        protected IReadOnlyDIContainer DIContainer { get; set; }

        protected void InitializeFactory(IReadOnlyDIContainer container, IResolver<T> resolver)
        {
            DIContainer = container;
            Resolver = resolver;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Factory<T> : AbstractFactory<T>, IFactory<T>
    {
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(IReadOnlyDIContainer container, IResolver<T> resolver)
            => InitializeFactory(container, resolver);
        public virtual async ValueTask<T> CreateAsync()
            => await Resolver.ResolveAsync(DIContainer);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Factory<TArg1, T> : AbstractFactory<T>, IFactory<TArg1, T>
    {
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(IReadOnlyDIContainer container, IResolver<T> resolver)
            => InitializeFactory(container, resolver);
        public virtual async ValueTask<T> CreateAsync(TArg1 arg1)
            => await Resolver.ResolveAsync(DIContainer, new object[] { arg1 });
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Factory<TArg1, TArg2, T> : AbstractFactory<T>, IFactory<TArg1, TArg2, T>
    {
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(IReadOnlyDIContainer container, IResolver<T> resolver)
            => InitializeFactory(container, resolver);
        public virtual async ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2)
            => await Resolver.ResolveAsync(DIContainer, new object[] { arg1, arg2 });
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Factory<TArg1, TArg2, TArg3, T> : AbstractFactory<T>, IFactory<TArg1, TArg2, TArg3, T>
    {
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(IReadOnlyDIContainer container, IResolver<T> resolver)
            => InitializeFactory(container, resolver);
        public virtual async ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3)
            => await Resolver.ResolveAsync(DIContainer, new object[] { arg1, arg2, arg3 });
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Factory<TArg1, TArg2, TArg3, TArg4, T> : AbstractFactory<T>, IFactory<TArg1, TArg2, TArg3, TArg4, T>
    {
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(IReadOnlyDIContainer container, IResolver<T> resolver)
            => InitializeFactory(container, resolver);
        public virtual async ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
            => await Resolver.ResolveAsync(DIContainer, new object[] { arg1, arg2, arg3, arg4 });
    }
}