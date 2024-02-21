using System;
using System.Collections.Generic;
using System.Reflection;

namespace Doinject
{
    internal class TargetFieldsInfo
    {
        public List<FieldInfo> InjectFields { get; } = new();

        public TargetFieldsInfo(Type targetType)
        {
            foreach (var fieldInfo in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fieldInfo.GetCustomAttributes(typeof(InjectAttribute), true).Length <= 0)
                    continue;

                if (!fieldInfo.IsPublic)
                    throw new Exception($"Field must be public. {targetType.Name}.{fieldInfo.Name}");
                InjectFields.Add(fieldInfo);
            }
        }

        public bool Any()
            => InjectFields.Count > 0;
    }
}