using System.Collections.Generic;
using UnityEngine;

namespace Doinject.Tests
{
    public class TestMonoBehaviour : MonoBehaviour, IInjectableComponent
    {
        public List<InjectedObject> InjectedObjects { get; private set; } = new();
        public int InjectedCount { get; private set; }

        [Inject]
        public void Inject(
            InjectedObject injectedObject,
            [Optional] InjectedObject injectedObject2,
            [Optional] InjectedObject injectedObject3,
            [Optional] InjectedObject injectedObject4)
        {
            InjectedObjects.Add(injectedObject);
            InjectedObjects.Add(injectedObject2);
            InjectedObjects.Add(injectedObject3);
            InjectedObjects.Add(injectedObject4);
            InjectedCount++;
        }
    }
}