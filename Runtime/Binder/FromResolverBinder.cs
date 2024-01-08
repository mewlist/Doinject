namespace Doinject
{
    public struct FromResolverBinder<T, TResolver> : IBinder
        where TResolver : IResolver<T>, new()
    {
        private BinderContext context;
        private object[] args;

        public FromResolverBinder(BinderContext context)
        {
            this.context = context;
            this.args = null;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new FromResolverResolver<T, TResolver>(args);
        }

        public FromResolverBinder<T, TResolver> Args(object[] args)
        {
            var clone = this;
            clone.args = args;
            clone.context.Update(clone);
            return clone;
        }
    }
}