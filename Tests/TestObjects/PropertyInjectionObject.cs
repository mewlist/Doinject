namespace Doinject.Tests
{
    internal class PropertyInjectionObject
    {
        [Inject] public InjectedObject InjectedObject { get; set; }
    }
}