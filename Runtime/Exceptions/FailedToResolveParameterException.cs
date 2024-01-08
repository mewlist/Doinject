using System;
using System.Reflection;

namespace Doinject
{
    public class FailedToResolveParameterException : Exception
    {
        public FailedToResolveParameterException(Type parameterType, MemberInfo targetType, Exception inner = null)
            : base($"Failed to resolve parameter [{parameterType.Name}] for [{targetType.Name}]".ToExceptionMessage(inner), inner)
        {
        }
    }
}