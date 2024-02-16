using UnityEngine;

namespace Doinject.Tests
{
    public class TestMonoBehaviour : MonoBehaviour, IInjectableComponent
    {
        public InjectedObject InjectedObject { get; private set; }
        public int InjectedCount { get; private set; }

        [Inject]
        public void Inject(InjectedObject injectedObject)
        {
            InjectedObject = injectedObject;
            InjectedCount++;
        }
    }
}