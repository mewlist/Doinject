using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Doinject
{
    public class BindingInstallerComponent : MonoBehaviour, IBindingInstaller
    {
        [field: SerializeField]
        protected List<BindingInstallerScriptableObject> InstallerScriptableObjects { get; set; }

        [field: SerializeField]
        protected List<MonoBehaviour> ComponentBindings { get; set; }

        public virtual void Install(DIContainer container, IContextArg contextArg)
        {
            foreach (var bindingScriptableObjectInstaller in InstallerScriptableObjects)
                bindingScriptableObjectInstaller.Install(container, contextArg);

            if (!ComponentBindings.Any()) return;


            var type = container.GetType();
            var methods = type.GetMethods();
            var method = methods.FirstOrDefault(x
                => x.Name == "BindFromInstance" &&
                   x.IsGenericMethodDefinition &&
                   x.GetGenericArguments().Length == 1);

            foreach (var component in ComponentBindings)
            {
                var componentType = component.GetType();
                var genericMethod = method.MakeGenericMethod(componentType);
                genericMethod.Invoke(container, new object[] { component });
            }
        }
    }
}