# Injection

This section explains where to inject dependencies.

## Constructor Injection

Constructor injection is a method of injecting dependencies into the arguments of the constructor.

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

Method injection is a method of injecting dependencies by calling a method with the [Inject] attribute.

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

## OnInjected() Callback

The ```OnInjected()``` method (no arguments, public) is automatically called when the instance's injection is complete. It can be an asynchronous function with a return value of ```Task```, ```ValueTask```, and there will be no problem.

Especially in the case of components that inherit MonoBehaviour, the timing of lifecycle methods such as Awake and Start and the injection may be reversed. ```OnInjected()``` is designed to be called in the next frame after the injection is complete, so it can be used to stabilize the initialization order.

```C#
public class SomeClass
{
    // Methods with the [Inject] attribute are called by the DI container after the instance is created, passing SomeDependency as an argument
    [Inject]
    public InjectMethod(SomeDependency dependency)
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

## Optional Injection

By specifying the ```[Optional]``` attribute as an argument, you can skip the injection if the dependency cannot be resolved. The skipped argument will receive the ```default``` value of its type.

```C#
public class SomeClass
{
    // If the dependency of SomeDependency cannot be resolved, null is passed
    public SomeClass([Optional] SomeDependency dependency)
    {
        ...
    }
}
```

## Dynamic Injection to Components (DynamicInjectable)

Normally, if you create a component without going through a Factory, such as using the usual Instantiate after creating the context space, the component will not be injected.

In such cases, you can usually perform the injection by creating an instance using a Factory, but it's a hassle to prepare a Factory every time.

In such cases, if you attach the ```DynamicInjectable``` component to the GameObject in advance, you can inject into the component that inherits IInjectableComponent attached to the same object, even when the instance is created with ```Object.Instantiate```.