namespace Doinject.Tests
{
    internal class OptionalInjectionTestClass
    {
        public OptionalInjectionTestClass([Optional]InjectedObject injectedObject)
        {
        }
    }
}