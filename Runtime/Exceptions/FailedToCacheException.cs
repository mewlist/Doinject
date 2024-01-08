using System;

namespace Doinject
{
    public class FailedToCacheException : Exception
    {
        public FailedToCacheException(IInternalResolver target, Exception inner = null)
            : base($"Failed to cache of [{target.Name}].".ToExceptionMessage(inner), inner)
        {
        }
    }
}