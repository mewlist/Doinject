# Creating an Installer as a Component

When creating an installer as a component, please follow the steps below. The installer component can be directly placed in the scene or used as a prefab.

## Creating an Installer Script

An installer can be created from the `Doinject` menu. Right-click in the Project view and select `Create` > `Doinject` > `Binding Installer Component C# Script` to create an installer script.

![SceneContext](CreateComponentBindingInstaller.png)

If you create a script with the name CustomComponentBindingInstallerScript.cs, a script like the following will be created.

```C#
using Doinject;

public class CustomComponentBindingInstallerScript : BindingInstallerComponent
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        // Bind your dependencies here
    }
}
```

Write your bindings in ```Install()```.

## Installing the Created Installer

The installer component can be installed by any of the following methods:

* Directly placed in a scene where a context entry point exists
* Directly placed in a scene that is loaded via a scene loader
* Placed under a game object context
* Placed at the root of a prefab and set in the project context, or another installer component