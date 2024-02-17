# Creating an Installer as a ScriptableObject

If you want to create an installer as a ScriptableObject, please follow the steps below.

## Creating the Installer Script

You can create an installer as a ScriptableObject from the `Doinject` menu. Right-click in the Project view, and select `Create` > `Doinject` > `Binding Installer ScriptableObject C# Script`. This will create the installer script.

![CreateScriptableObjectInstaller](CreateBindingInsallerScriptableObjectScript.png)

If you create a script with the name CustomBindingInstallerScriptableObjectScript, a script like the one below will be created.

```C#
using Doinject;
using UnityEngine;

[CreateAssetMenu(menuName = "Doinject/Installers/CustomBindingInstallerScriptableObjectScript", fileName = "CustomBindingInstallerScriptableObjectScript", order = 0)]
public class CustomBindingInstallerScriptableObjectScript : BindingInstallerScriptableObject
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
    }
}
```
You will write your bindings inside the ```Install()``` method.

## Creating a ScriptableObject Asset

The ScriptableObject created here can be right-clicked in the Project view and selected to create a ScriptableObject asset under `Create` > ```Doinject``` > ```Installers```.

## Installing the ScriptableObjectInstaller 

From the installer component's inspector, you can set the installer's ScriptableObject as follows.

![InstallScriptableObjectInstaller](InstallScriptableObjectInstaller.png)