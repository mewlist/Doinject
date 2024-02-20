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
            foreach (var propertyInfo in targetType.GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                    InjectProperties.Add(propertyInfo);
            }
        }

        public bool Any()
            => InjectProperties.Count > 0;
    }
}