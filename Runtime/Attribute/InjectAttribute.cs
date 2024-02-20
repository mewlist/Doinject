using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Constructor|AttributeTargets.Method|AttributeTargets.Field|AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}