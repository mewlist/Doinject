using System;
using UnityEngine.TestTools;

namespace Doinject.Tests
{
    public class ExceptionTestScope : IDisposable
    {
        private readonly bool currentValue;

        public ExceptionTestScope()
        {
            currentValue = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
        }

        public void Dispose()
        {
            LogAssert.ignoreFailingMessages = currentValue;
        }
    }
}