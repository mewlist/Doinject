namespace Doinject
{
    public struct InstanceBinder<T, TInstance> : IBinder
        where TInstance : T
    {
        private BinderContext context;
        private readonly T instance;

        public InstanceBinder(BinderContext context, TInstance instance)
        {
            context.ValidateInstanceType(typeof(TInstance));
            this.context = context;
            this.instance = instance;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return new InstanceResolver<T>(instance);
        }
    }
}