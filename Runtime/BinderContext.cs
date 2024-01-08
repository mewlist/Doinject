using System;

namespace Doinject
{
    public sealed class BinderContext
    {
        private IBinder Binder { get; set; }

        public void Update(IBinder binder)
        {
            Binder = binder;
        }

        public IInternalResolver ToResolver(InstanceBag instanceBag)
        {
            return Binder.ToResolver(instanceBag);
        }

        public void ValidateInstanceType(Type instanceType)
        {
            if (instanceType.IsInterface)
                throw new Exception($"Instance type [{instanceType.Name}] is interface".ToExceptionMessage());
        }
    }
}