using System;

namespace Doinject.Tests
{
    internal class InjectableObject : IDisposable
    {
        public InjectedObject InjectedObject { get; set; }

        public InjectableObject(InjectedObject injectedObject)
        {
            InjectedObject = injectedObject;
        }

        public void Dispose()
        { }
    }
}