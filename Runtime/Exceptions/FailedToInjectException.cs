using System;

namespace Doinject
{
    public class FailedToInjectException : Exception
    {
        public FailedToInjectException(object target, Exception inner = null)
            : base($"Failed to inject into [{target.GetType().Name}].".ToExceptionMessage(inner), inner)
        {
        }
    }
}