namespace Doinject
{
    public interface IBinder
    {
        IInternalResolver ToResolver(InstanceBag instanceBag);
    }
}