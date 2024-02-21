# Injection

This section explains where dependencies are injected.

## Constructor Injection

Constructor injection is a method of injecting dependencies into the arguments of a constructor.

```C#
public class SomeClass
{
    // When SomeClass is instantiated, SomeDependency is automatically injected
    public SomeClass(SomeDependency dependency)
    {
        ...
    }
}
```

## Method Injection

Method injection is a method of injecting dependencies by calling a method with the ```[Inject]``` attribute.

```C#
public class SomeClass
{
    // Methods with the [Inject] attribute are called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## Field Injection

Field injection is a method of injecting dependencies into ```public``` fields with the ```[Inject]``` attribute.

```C#
public class SomeClass
{
    // Inject SomeDependency into a public field with the [Inject] attribute
    [Inject] public SomeDependency dependency;
    ...
}
```

## Property Injection

Property injection is a method of injecting dependencies into properties with a ```public``` setter and the ```[Inject]``` attribute.

```C#
public class SomeClass
{
    // Inject SomeDependency into a property with a public setter and the [Inject] attribute
    [Inject] public SomeDependency Dependency {get; set;}
    ...
}
```

## OnInjected() Callback


The method ```OnInjected()``` (no arguments, public) is automatically called when the injection of an instance is completed. It can be an asynchronous function with a return value of ```Task``` or ```ValueTask```.

In particular, for components that inherit from MonoBehaviour, the timing of lifecycle methods such as Awake or Start and the timing of injections may change. ```OnInjected()``` is designed to be called on the frame after injection is complete, so it can be used to stabilize the initialization order.

```C#
public class SomeClass
{
    // Methods with the [Inject] attribute are called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
    
    // This is automatically called when the injection of an instance is completed
    [OnInjected]
    public void OnInjected()
    {
        ...
    }
}
```

## Optional Injection

By specifying the ```[Optional]``` attribute as an argument, if the dependency cannot be resolved, the injection can be skipped. The ```default``` value of the type is passed to skipped arguments.

```C#
public class SomeClass
{
    // If the dependency on SomeDependency cannot be resolved, null is passed
    public SomeClass([Optional] SomeDependency dependency)
    {
        ...
    }
}
```

## Dynamic Injection to Components (DynamicInjectable)

Normally, injections are not performed on components created without going through a Factory, such as using the regular Instantiate, after the context space is created.

In such cases, you can perform injections by using a Factory to create instances, but it can be a hassle to prepare a Factory every time.

In such cases, you can attach the ```DynamicInjectable``` component to the GameObject in advance, and perform injections to components that inherit IInjectableComponent attached to the same object, even at the timing of instance creation with ```Object.Instantiate```.