using UnityEngine;

namespace Doinject.Tests
{
    public class PropertyInjectionComponent : MonoBehaviour, IInjectableComponent
    {
        [Inject]
        public InjectedObject InjectedObject { get; set; }
    }
}