# Getting Started with Doinject

## Installing via Unity Package Manager

You can install it from the Unity Package Manager.

### Installing MewCore

<procedure id="procedure-install-mewcore">
<p>First, please install the MewCore package, which is a dependent library.</p>
<step>Select <ui-path>Window > Package Manager</ui-path> from the Unity menu.</step>
<step>Click the <control>+</control> button and select <control>Add package from git URL...</control>.</step>
<step>Enter the following and click <control>Add</control>.
<tabs>
<tab title="https">
<code-block>https://github.com/mewlist/MewCore.git</code-block>
</tab>
<tab title="ssh">
<code-block>git@github.com:mewlist/MewCore.git</code-block>
</tab>
</tabs>
</step>
</procedure>

### Installing Doinject

<procedure id="procedure-install-doinject">
<p>Next, install the Doinject package.</p>
<step>Select <ui-path>Window > Package Manager</ui-path> from the Unity menu.</step>
<step>Click the <control>+</control> button and select <control>Add package from git URL...</control>.</step>
<step>Enter the following and click <control>Add</control>.
<tabs>
<tab title="https">
<code-block>https://github.com/mewlist/Doinject.git</code-block>
</tab>
<tab title="ssh">
<code-block>git@github.com:mewlist/Doinject.git</code-block>
</tab>
</tabs>
</step>
</procedure>


## Determining the Entry Point

The first thing to do is to open the scene where you want to apply the DI framework and place the ```SceneContext``` component in the hierarchy.

Right-click in the hierarchy view and select the <ui-path>Doinject > Create Scene Context</ui-path> menu.

![ContextMenu](CreateSceneContext.png)

Scene context that has been placed

![SceneContext.png](SceneContext.png)

Now, this scene functions as an execution environment for the DI framework.
Please try playing it once.

![SceneContextCreated.png](SceneContextCreated.png)

When the scene is played, various loaders are generated like this.

> Doinject can only access its functions within the "context space".
> The "context space" is defined for scenes and game objects, and the game object context has a hierarchical space according to the hierarchy.
> Each context has a DI container and a scene loader hanging from it, functioning as an execution environment for the DI framework.
{style="note"}

## First Injection

Let's register an instance in the DI container and perform an injection.
Instance registration (also known as binding) is done from a component called an installer.

### About Injection Points

There are several ways to inject a type registered in the DI container into a specific instance:

: * Constructor Injection
: * Method Injection
: * Field Injection
: * Property Injection

Constructor injection is automatically performed when an instance is created through the DI container.
Method injection is performed by calling a method with the [Inject] attribute.
Property and field injection inject an instance to a field or property with the [Inject] attribute.

> Instances created through the DI container will automatically attempt dependency injection on their constructors, so the [Inject] attribute is not necessary.
> It is not a problem to explicitly add the [Inject] attribute, but you need to consider the fact that you may become dependent on that attribute definition.
{style="note"}

In the case of components that inherit from MonoBehaviour, constructor injection cannot be used.
This is because the generation of components is done by Unity, so you cannot explicitly call the constructor.
Therefore, you need to use method injection, field injection, or property injection.

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
    // A method with the [Inject] attribute is called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public void InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## About the OnInjected() Callback

A method (no arguments, public) with the ```[OnInjected]``` attribute will automatically be called when the instance's injection is complete.
It can also be an asynchronous function with a ```Task``` or ```ValueTask``` return value.

Especially for components that inherit from MonoBehaviour, the timing of lifecycle methods such as Awake or Start and the timing of injections may be reversed.
The ```[OnInjected]``` callback is designed to be called on the next frame after the injection is complete, so it can be used to stabilize the initialization order.

```C#
class SomeClass
{
    public SomeClass(SomeDependency dependency)
    {
        ...
    }

    // It is automatically called when the injection of the instance is complete
    [OnInjected]
    public void OnInjected()
    {
        ...
    }
}
```

## Creating an Installer Script

You can create an installer from the `Doinject` menu. By right-clicking in the Project view and selecting `Create` > `Doinject` > `Component Binding Installer C# Script`, an installer script is created.

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

### Script to Inject
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

The goal is for the instance of the Player object to be injected through `InjectTargetScript.Construct()`.

> Types that inherit from MonoBehaviour and are defined as components need to inherit from IInjectableComponent.
> Also, the method to be injected with the [Inject] attribute must be public.
{style="note"}

Now, let's describe the binding in the installer.
Bindings are done in the Install method.

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

Bindings are done through a Fluent Interface like this.
Depending on how you describe it, you can define flexible behaviors such as acting as a factory or getting instances through an interface.

> Although we use the term Singleton for convenience, it represents the concept of being unique within the context space.
> Unlike the singleton pattern, instances are not shared beyond the boundaries of the context.
{style="note"}

Now, a Player object, which is unique within the context, has been registered in the DI container.

Finally, let's place the installer in the scene.

![](InstallerPlacedOnScene.png)

Component installers basically function wherever they are placed in the scene.
Also, it doesn't matter if there are multiple different installers.
It's a good idea to set rules such as placing them under the entry point for clarity.

## Checking the Operation

Let's place the InjectTargetScript component we made earlier in the scene.

![](InjectTargetPlacedOnScene.png)

Now, let's play the scene.

![](InjectTargetInjected.png)

You can confirm that the name of the Player is set to "Novice Player".

The argument specified by Args was passed to the constructor of Player (constructor injection), and the Player object was passed to the Construct method of InjectTargetScript (method injection).

> At this time, you can think of the following process being performed inside the DI container.
> ```C#
> var arg = "Novice Player";
> var player = new Player(arg);
> var injectable = Object.FindComponents(typeof(IInjectableComponent)).First();
> (injectable as InjectTargetScript).Construct(player)
> ```
{style="note"}

### Supplement

Objects placed throughout the scene are all covered by the scene context.
: * The scene context, when created, searches for installers belonging to its own context and performs bindings according to their definitions.
: * Once all the installers have completed their bindings, the DIContainer is in a state where it knows the types it should handle and how to resolve instances.
: * After that, the scene context searches for all game objects in the scene and looks for components that implement IInjectableComponent.
: * When a component implementing IInjectableComponent is found, it calls the method with the [Inject] attribute specified on that component to pass an instance that matches the argument and type.
: * The scene context is created in sync with the scene lifecycle and is destroyed along with the instances held by the container when the scene is destroyed.