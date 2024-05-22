namespace Doinject.Tests
{
    internal class ConstructorInjectionTestClass
    {
        public InjectedObject InjectedObject { get; set; }

        public ConstructorInjectionTestClass(InjectedObject injectedObject)
        {
            InjectedObject = injectedObject;
        }
    }
}