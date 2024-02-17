using System;

namespace Doinject
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class PostInjectAttribute : Attribute
    {
    }
}