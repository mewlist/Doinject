namespace Doinject
{
    public class FixedBinder : IBinder
    {
        private readonly IBinder actualBinder;

        public FixedBinder(BinderContext context, IBinder actualBinder)
        {
            this.actualBinder = actualBinder;
            context.Update(this);
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return actualBinder.ToResolver(instanceBag);
        }
    }
}