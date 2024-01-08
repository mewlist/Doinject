using System;
using System.Linq;
using UnityEngine;

namespace Doinject
{
    public readonly struct TargetTypeInfo
    {
        public Type Type { get; }
        public bool IsMonoBehaviour { get; }
        public bool IsGameObject { get; }
        public bool MayBePrefab { get; }
        public bool IsValid => Type is not null;

        public TargetTypeInfo(Type type)
        {
            Type = type;
            IsMonoBehaviour = type.IsSubclassOf(typeof(MonoBehaviour));
            IsGameObject = type == typeof(GameObject);
            MayBePrefab = IsGameObject || IsMonoBehaviour;
        }

        public bool IsSubclassOf(Type type)
        {
            return Type.IsSubclassOf(type);
        }

        public TargetTypeInfo FindFactoryInterfaces()
        {
            var factoryTypes = new []
            {
                typeof(IFactory<>),
                typeof(IFactory<,>),
                typeof(IFactory<,,>),
                typeof(IFactory<,,,>),
                typeof(IFactory<,,,,>)
            };
            var interfaces = Type.FindInterfaces(
                (type, criteria)
                    => type.IsGenericType && ((Type[])criteria).Contains(type.GetGenericTypeDefinition()),
                factoryTypes);
            return interfaces.Length == 0
                ? default
                : new TargetTypeInfo(interfaces.First());
        }
    }
}