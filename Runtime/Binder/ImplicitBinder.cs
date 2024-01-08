using System;

namespace Doinject
{
    public readonly struct ImplicitBinder<T> : IBinder
    {
        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            throw new Exception("Instance will be resolved implicitly in factory");
        }
    }
}