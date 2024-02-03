using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class PostInjectAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnInjectedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class OptionalAttribute : Attribute
    {
    }
}