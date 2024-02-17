using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OnInjectedAttribute : Attribute
    {
    }
}