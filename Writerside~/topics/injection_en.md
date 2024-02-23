# Injection

This section explains where to inject dependencies.

## Constructor Injection

Constructor injection is a method of injecting dependencies into the arguments of a constructor.

```C#
public class SomeClass
{
    // When SomeClass is created, SomeDependency is automatically injected
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
    // A method with the [Inject] attribute is called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## Field Injection

Field injection is a method of injecting dependencies into a ```public``` field with the ```[Inject]``` attribute.

```C#
public class SomeClass
{
    // Injects SomeDependency into a public field with the [Inject] attribute
    [Inject] public SomeDependency dependency;
    ...
}
```

## Property Injection

Property injection is a method of injecting dependencies into a property with a ```public``` setter and the ```[Inject]``` attribute.

```C#
public class SomeClass
{
    // Injects SomeDependency into a property with a public setter and the [Inject] attribute
    [Inject] public SomeDependency Dependency {get; set;}
    ...
}
```

## OnInjected Callback

A method with the ```OnInjected``` attribute (no arguments, public) is automatically called when the instance's injection is complete. It's no problem even if it's an asynchronous function with return values of ```Task```, ```ValueTask```, or ```UniTask```.

Especially for components inheriting MonoBehaviour, the timing of lifecycle methods like Awake or Start and the injection may overlap. The ```OnInjected``` callback is designed to be called in the frame after the injection is complete, used to stabilize the initialization of the component.

```C#
public class SomeClass
{
    // A method with the [Inject] attribute is called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
    
    // A method with the [OnInjected] attribute is automatically called when the instance's injection is complete
    [OnInjected]
    public void OnInjected()
    {
        ...
    }
}
```

## Optional Injection

By specifying the ```[Optional]``` attribute for an argument, you can skip the injection if the dependency cannot be resolved. The ```default``` value of the type is passed to skipped arguments.

```C#
public class SomeClass
{
    // If SomeDependency cannot be resolved, null is passed
    public SomeClass([Optional] SomeDependency dependency)
    {
        ...
    }
}
```

> This allows an object that requires a specific context to function in another context. By setting default values when the injection is skipped, you can ensure the object's operation and improve debugging efficiency. It's also useful for realizing objects whose operation switches depending on the context.
{style="note"}

## Dynamic Injection to Components (DynamicInjectable)

After the creation of the context space, when a component is created without going through a Factory, such as using a regular Instantiate, the component is not normally injected.

In such cases, you can perform the injection by creating instances using a Factory, but it's also a hassle to prepare a Factory every time.

In such cases, if you attach the ```DynamicInjectable``` component to the GameObject in advance, you can inject into the components that inherit IInjectableComponent attached to the same object, even when instances are created with ```Object.Instantiate```.

This supports development following the regular coding flow of Unity while using the DI container.