namespace Doinject
{
    public struct FactoryBinder<T, TFactory> : IBinder
        where TFactory : IFactory
    {
        private readonly BinderContext context;
        private readonly IBinder innerBinder;
        private object[] args;

        public FactoryBinder(BinderContext context, IBinder innerBinder)
        {
            this.context = context;
            this.innerBinder = innerBinder;
            this.args = null;
            context.Update(this);
        }

        public FactoryBinder<T, TFactory> Args(params object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            if (typeof(AbstractFactory<T>).IsAssignableFrom(typeof(TFactory)))
            {
                var innerResolver = (IResolver<T>)innerBinder.ToResolver(instanceBag);
                return new FactoryResolver<T, IResolver<T>, TFactory>(innerResolver, instanceBag);
            }

            return new CustomFactoryResolver<TFactory>(instanceBag, args);
        }
    }
}