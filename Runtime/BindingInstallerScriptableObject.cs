using UnityEngine;

namespace Doinject
{
    public abstract class BindingInstallerScriptableObject : ScriptableObject, IBindingInstaller
    {
        public abstract void Install(DIContainer container);
    }
}