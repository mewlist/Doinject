using System;
using System.Linq;
using UnityEngine.Assertions;

namespace Doinject
{
    public readonly struct ScopedInstance
    {
        public Type TargetType { get; }
        public object Instance { get; }
        public bool IsValid => TargetType is not null && Instance is not null;

        public ScopedInstance(object instance)
        {
            TargetType = instance.GetType();
            Instance = instance;
        }

        public ScopedInstance(Type targetType, object instance)
        {
            var instanceType = instance.GetType();
            Assert.IsTrue(targetType.IsInterface
                ? instanceType.GetInterfaces().Contains(targetType)
                : instanceType.IsSubclassOf(targetType));
            TargetType = targetType;
            Instance = instance;
        }
    }
}