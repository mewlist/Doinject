using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Constructor|AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}