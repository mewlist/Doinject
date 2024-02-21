using System;
using System.Collections.Generic;
using System.Reflection;

namespace Doinject
{
    internal class TargetPropertiesInfo
    {
        public List<PropertyInfo> InjectProperties { get; } = new();

        public TargetPropertiesInfo(Type targetType)
        {
            foreach (var propertyInfo in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (propertyInfo.GetCustomAttributes(typeof(InjectAttribute), true).Length <= 0)
                    continue;

                if (!propertyInfo.CanWrite || !propertyInfo.SetMethod.IsPublic)
                    throw new Exception($"Property must have a public setter. {targetType.Name}.{propertyInfo.Name}");

                InjectProperties.Add(propertyInfo);
            }
        }

        public bool Any()
            => InjectProperties.Count > 0;
    }
}