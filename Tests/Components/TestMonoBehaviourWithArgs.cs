using UnityEngine;

namespace Doinject.Tests
{
    public class TestMonoBehaviourWithArgs : MonoBehaviour, IInjectableComponent
    {
        public InjectedObject InjectedObject { get; private set; }
        public int Arg1 { get; private set; }
        public string Arg2 { get; private set; }

        [Inject]
        public void Inject(int arg1, InjectedObject injectedObject, string arg2)
        {
            InjectedObject = injectedObject;
            Arg1 = arg1;
            Arg2 = arg2;
        }

    }
}