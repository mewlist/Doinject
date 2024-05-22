namespace Doinject.Tests
{
    internal class FieldInjectionObject
    {
        [Inject] public InjectedObject injectedObject;
    }

    internal class FieldInjectionWithNonPublicObject
    {
        [Inject] private InjectedObject injectedObject;
    }
}