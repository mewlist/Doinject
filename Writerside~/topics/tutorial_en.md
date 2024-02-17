# Getting Started with Doinject

## Installing via Unity Package Manager

You can install it from the Unity Package Manager.

### Installing MewCore

<procedure id="procedure-install-mewcore">
<p>First, please install the MewCore package, which is a dependency library.</p>
<step>Select <ui-path>Window > Package Manager</ui-path> from the Unity menu.</step>
<step>Click the <control>+</control> button and select <control>Add package from git URL...</control>.</step>
<step>Enter the following and click <control>Add</control>.
<code-block>
git@github.com:mewlist/MewCore.git
</code-block>
</step>
</procedure>

### Installing Doinject

<procedure id="procedure-install-doinject">
<p>Next, install the Doinject package.</p>
<step>Select <ui-path>Window > Package Manager</ui-path> from the Unity menu.</step>
<step>Click the <control>+</control> button and select <control>Add package from git URL...</control>.</step>
<step>Enter the following and click <control>Add</control>.
<code-block>
git@github.com:mewlist/Doinject.git
</code-block>
</step>
</procedure>

## Setting the Entry Point

The first thing to do is to open the scene where you want to apply the DI framework and place a ```SceneContext``` component in the hierarchy.

Right-click in the Hierarchy view and select the <ui-path>Doinject > Create Scene Context</ui-path> menu.

![ContextMenu](CreateSceneContext.png)

The placed scene context

![SceneContext.png](SceneContext.png)

Now, this scene functions as an execution environment for the DI framework.
Try playing it once.

![SceneContextCreated.png](SceneContextCreated.png)

When the scene is played, various loaders are generated like this.

> Doinject can only access its features within the "context space".
> The "context space" is defined for scenes and game objects, and the game object context has a space that follows the hierarchy of the hierarchy.
> Each context has a DI container and a scene loader hanging from it, functioning as an execution environment for the DI framework.
{style="note"}

## Your First Injection

Register an instance in the DI container and try injecting it.
Instance registration (also known as binding) is performed from a component called an installer.

### About Injection Points

There are two ways to inject a type registered in the DI container into a specific instance:

: * Constructor injection
: * Method injection

Constructor injection is automatically performed when an instance is created through the DI container.
Method injection is performed by calling a method with the [Inject] attribute.

> Instances created through the DI container automatically attempt dependency injection in their constructors, so the [Inject] attribute is not necessary.
> It's not a problem to explicitly add the [Inject] attribute, but you need to consider the dependence on that attribute definition.
{style="note"}

In the case of components that inherit MonoBehaviour, you cannot use constructor injection.
This is because Unity creates the component and you cannot explicitly call the constructor.
Therefore, you always need to use method injection.

### Constructor Injection

```C#
class SomeClass
{
    // When SomeClass is created, SomeDependency is automatically injected
    public SomeClass(SomeDependency dependency)
    {
        ...
    }
}
```

### Method Injection

```C#
class SomeClass
{
    // Methods with the [Inject] attribute are called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## About the OnInjected() Callback

Methods with the ```[OnInjected]``` attribute (no arguments, public) are automatically called when the instance injection is complete.
It's not a problem if it's an asynchronous function with a ```Task``` or ```ValueTask``` return value.

Especially for components that inherit MonoBehaviour, there is a possibility that the timing of lifecycle methods such as Awake and Start and the timing of injection will be reversed.
The ```[OnInjected]``` callback is designed to be called on the next frame after the injection is complete, so it can be used for the purpose of stabilizing the initialization order.

```C#
class SomeClass
{
    public SomeClass(SomeDependency dependency)
    {
        ...
    }

    // This is automatically called when the instance injection is complete
    [OnInjected]
    public void OnInjected()
    {
        ...
    }
}
```

## Creating an Installer Script

An installer can be created from the `Doinject` menu. Right-click in the Project view and select
`Create` > `Doinject` > `Component Binding Installer C# Script` to create an installer script.

![SceneContext](CreateComponentBindingInstaller.png)

If you create a script named CustomComponentBindingInstallerScript.cs, a script like the following will be created.

```C#
using Doinject;

public class CustomComponentBindingInstallerScript : BindingInstallerComponent
{
    public override void Install(DIContainer container)
    {
        // Bind your dependencies here
    }
}
```

Also, to verify the operation, we will create an object to register in the DI container and a script to inject as follows.

### Player Object
```C#
using System;
using UnityEngine;

[Serializable]
public class Player
{
    [SerializeField] public string name;
    public Player(string name)
    {
        this.name = name;
    }
}
```

### Injection Target Script
```C#
using Doinject;
using UnityEngine;

public class InjectTargetScript : MonoBehaviour, IInjectableComponent
{
    [SerializeField] private Player player;

    [Inject] public void Construct(Player player)
    {
        this.player = player;
    }
}
```

The goal is for an instance of the Player object to be injected through `InjectTargetScript.Construct()`.

> Types that inherit MonoBehaviour and are defined as components need to inherit IInjectableComponent.
> Also, the method to which the [Inject] attribute is attached, which is the injection destination, needs to be public.
{style="note"}

Now, let's write the binding in the installer.
The binding is done in the Install method.

```C#
    public override void Install(DIContainer container)
    {
        container
            .Bind<Player>()
            .Args("Novice Player")
            .AsSingleton();
    }
```

`.Bind<Player>()` is a declaration to get the Player class through the container.
`.Args("Novice Player")` is an argument to pass to the constructor of the Player class.
`.AsSingleton()` indicates that the Player object provided through the container is a singleton.

Binding descriptions are done through a Fluent Interface like this.
Depending on how you describe it, you can define flexible behaviors, such as acting as a factory or getting instances through an interface.

> We use the term Singleton for convenience, but it represents the concept of being unique within the context space.
> Unlike the singleton pattern, instances are not shared across context boundaries.
{style="note"}

Now, a Player object that is unique within the context has been registered in the DI container.

Finally, let's place the installer in the scene.

![](InstallerPlacedOnScene.png)

The component installer basically works wherever it is placed in the scene.
Also, it's okay to have multiple different installers.
For clarity, it's a good idea to set rules such as placing it under the entry point.

## Verification of Operation

Let's place the InjectTargetScript component we created earlier in the scene.

![](InjectTargetPlacedOnScene.png)

Now, let's play the scene.

![](InjectTargetInjected.png)

You can confirm that the name of Player is set to "Novice Player".

The argument specified by Args was passed to the constructor of Player (constructor injection), and
You can confirm that the Player object was passed to the Construct method of InjectTargetScript (method injection).

> At this time, you can think that the following processing is performed inside the DI container.
> ```C#
> var arg = "Novice Player";
> var player = new Player(arg);
> var injectable = Object.FindComponents(typeof(IInjectableComponent)).First();
> (injectable as InjectTargetScript).Construct(player)
> ```
{style="note"}

### Supplementary Information

Objects placed throughout the scene are all encompassed by the scene context.
: * The scene context, when created, searches for installers that belong to its own context and binds according to their definitions.
: * Once all installer bindings are complete, the DIContainer is in a state where it knows the types it should handle and how to resolve instances.
: * After that, the scene context searches all game objects in the scene and looks for components that implement IInjectableComponent.
: * When it finds a component that implements IInjectableComponent, it calls it to pass an instance that matches the arguments and types of the method specified by the [Inject] attribute.
: * The scene context is created in accordance with the scene lifecycle and is destroyed along with the instances held by the container when the scene is destroyed.