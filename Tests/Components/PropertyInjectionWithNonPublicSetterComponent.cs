using UnityEngine;

namespace Doinject.Tests
{
    public class PropertyInjectionWithNonPublicSetterComponent : MonoBehaviour, IInjectableComponent
    {
        [Inject]
        public InjectedObject InjectedObject { get; private set; }
    }
}