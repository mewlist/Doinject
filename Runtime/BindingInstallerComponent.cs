using System.Collections.Generic;
using UnityEngine;

namespace Doinject
{
    public class BindingInstallerComponent : MonoBehaviour, IBindingInstaller
    {
        [field: SerializeField]
        protected List<BindingInstallerScriptableObject> InstallerScriptableObjects { get; set; }

        public virtual void Install(DIContainer container, IContextArg contextArg)
        {
            foreach (var bindingScriptableObjectInstaller in InstallerScriptableObjects)
                bindingScriptableObjectInstaller.Install(container, contextArg);
        }
    }
}