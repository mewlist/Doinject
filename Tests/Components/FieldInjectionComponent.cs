using UnityEngine;

namespace Doinject.Tests
{
    public class FieldInjectionComponent : MonoBehaviour, IInjectableComponent
    {
        [Inject] public InjectedObject injectedObject;
    }
}