using System;
using System.Reflection;

namespace Doinject
{
    public class FailedToResolveException : Exception
    {
        public FailedToResolveException(MemberInfo targetType, Exception inner = null)
            : base($"Failed to resolve [{targetType.Name}].".ToExceptionMessage(inner), inner)
        {
        }
    }
}