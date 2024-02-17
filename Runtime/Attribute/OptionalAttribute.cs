using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OptionalAttribute : Attribute
    {
    }
}