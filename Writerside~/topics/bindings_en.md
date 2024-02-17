<show-structure for="chapter,procedure" depth="2"/>

# Binding

Registering types or instances with the DI container is called binding.

Instance provision methods

: * Singleton
: * Factory
: * Fixed instance
: * Temporary generation (Transient)

Types of objects

: * Type
: * Game object
: * Prefab
: * Addressable Asset

Other

: * Factory
: * Arguments

By combining these, you can perform various bindings flexibly with simple coding.

## Specifying the type to bind

### The simplest type binding

```C#
container.Bind<SomeClass>();
```

Register the type ```SomeClass``` in the DI container. An instance is created only once in the context space when there is an injection target and is cached (same as ```AsCached()```).

### Interface

```C#
container.Bind<ISomeInterface>()
    .To<SomeClass>();
```

```SomeClass``` needs to implement ```ISomeInterface```.
An instance of ```SomeClass``` is injected into ```ISomeInterface```.

You can also write it as follows.

```C#
container.Bind<ISomeInterface, SomeClass>();
```

### Type inherited from MonoBehaviour

```C#
container.Bind<SomeComponent>();
```

The coding for regular binding is the same.

> In type binding, you can bind types inherited from ```MonoBehaviour```.
> By default, a ```GameObject``` with the target component attached is created at the top level of the hierarchy of the scene to which the context belongs.
{style="note"}

You can specify the generation position as follows.

* Generate at the top level of the scene to which the context belongs

  ```C#
  container.Bind<SomeComponent>()
    .UnderSceneRoot();
  ```

* Generate as a child object of the specified ```Transfrom```
    
    ```C#
    container.Bind<SomeComponent>()
        .Under(someTransform);
    ```

* ```AddComponent``` to the specified ```GameObject```

    ```C#
    container.Bind<SomeComponent>()
        .On(someGameObject);
    ```


### Instance

```C#
container
    .Bind<SomeClass>()
    .FromInstance(someClassInstance);
```

someClassInstance is injected as is.

You can also write it as follows.

```C#
container.BindInstance(someClassInstance);
```

### Generate an instance from a prefab

```C#
container.BindPrefab<SomeComponent>(somePrefab);
```

SomeComponent needs to be attached to the root object of the prefab.

> By default, the target prefab is instantiated at the top level of the hierarchy of the scene to which the context belongs.
{style="note"}

You can specify the generation position as follows.

* Generate at the top level of the scene to which the context belongs

  ```C#
  container.Bind<SomeComponent>()
      .UnderSceneRoot();
  ```

* Generate as a child object of the specified ```Transfrom```

    ```C#
    container.BindPrefab<SomeComponent>(somePrefab)
      .Under(someTransform);
    ```


### Load from Addressable Asset {id="load-from-addressable-asset"}

```AsserReference``` is a type provided by the Addressable Asset System and represents a reference to an asset.

```C#
AssetReference assetReference = ...;
container.BindAssetReference<SomeAddressalbesObject>(assetReference);
```
You can also specify ```RuntimeKey``` directly using ```BindAssetRuntimeKey```.

```C#
string path_or_guid_of_asset = ...;
container.BindAssetRuntimeKey<SomeAddressalbesObject>(path_or_guid_of_asset);
```

> It is intended to be used for binding ScriptableObject and others.
{style="note"}

### Load a prefab from Addressables Asset and generate an instance

You can load a prefab from the Addressable Asset System and generate an instance using ```BindPrefabAssetReference```.
```PrefabAssetReference``` is a type provided by Doinject for referencing prefabs.
SomeComponent needs to be attached to the root object of the prefab.

```C#
PrefabAssetReference prefabAssetReference = ...;
container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);
```

You can also specify ```RuntimeKey``` directly using ```BindPrefabAssetRuntimeKey```.

```C#
string path_or_guid_of_prefab = ...;
container.BindPrefabAssetRuntimeKey<SomeComponent>(path_or_guid_of_prefab);
```

> By default, the target prefab is instantiated at the top level of the hierarchy of the scene to which the context belongs.
{style="note"}

You can specify the generation position as follows.

* Generate at the top level of the scene to which the context belongs

    ```C#
    container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
        .UnderSceneRoot();
    ```

* Generate as a child object of the specified ```Transfrom```

    ```C#
    container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
        .Under(someTransform);
    ```

## Specifying the instance generation method

### Cache

```C#
container.Bind<SomeClass>()
    .AsCached();
```

By calling ```AsCached()```, you explicitly indicate that it will be cached. Even if there is no coding, it is cached by default.
You can also write it as follows.

```C#
container.BindCached<SomeClass>();　// Abbreviation of the above
```

### Singleton

```C#
container.Bind<SomeClass>()
    .AsSingleton();

container.BindSingleton<SomeClass>();　// Abbreviation of the above
```

It generates only one instance in the context.
Unlike ```AsCached()```, it generates an instance even if there is no injection target.

> When binding a prefab or component, use it to place an instance in the scene or to generate an instance with some asynchronous function at the time of generation.
> If there is an injection target, only one instance is injected into the context, just like ```AsCached()```, so there is not much difference in behavior.
{style="note"}

### Temporary (Transient)

```C#
container.Bind<SomeClass>()
    .AsTransient();
```

Instances generated by AsTransient() are generated separately for each injection target.
Since the generated instance may not be automatically destroyed when the DI container is released, care is needed.


## Arguments

Assume that SomeClass has the following constructor.

```C#
public SomeClass(int someInt, string someString)
{
    ...
}
```

For such types, you need to pass arguments when generating instances,
You need to define arguments with Args().

```C#
container.Bind<SomeClass>()
    .Args(123, "some argument string");
```

> Arguments behave the same for method injection as well as constructor injection.
{style="note"}

### Injecting another bound type

Assume that SomeClass has the following constructor.

```C#
public SomeClass(ISomeInterface someInterface)
{
    ...
}
```

And, ISomeInterface is bound.

```C#
container.Bind<ISomeInterface>()
    .To<SomeOtherClass>();

container.Bind<SomeClass>();
```

In this case, when ```SomeClass``` is generated, an instance of ```SomeOtherClass``` is automatically injected into ```ISomeInterface```.

Next, consider a case with a more complex constructor like the following.

```C#
public SomeClass(ISomeInterface someInterface, int someInt, string someString)
{
    ...
}
```

By specifying the arguments that the DI container cannot resolve with ```Args()```, it is possible to generate instances.

```C#
container.Bind<ISomeInterface>()
    .To<SomeOtherClass>();

container.Bind<SomeClass>()
    .Args(123, "some argument string");
```

Injection is performed for arguments that the DI container cannot resolve according to the order of arguments specified in Args.
Therefore, even for the following constructor, injection is possible without any problem with the above binding description.

```C#
public SomeClass(int someInt, ISomeInterface someInterface, string someString)
{
    ...
}
```

## Factory

The way to generate a factory is to just call ```AsFactory()``` on the binding description. 
```IFactory<T>``` corresponding to the binding description is automatically bound.

```C#
container.Bind<SomeClass>()
    .AsFactory();
```

By describing this way, ```IFactory<SomeClass>``` is bound.

By defining a factory, a factory that generates the specified instance can be injected.
You can generate an instance by calling the ```CreateAsync()``` method on the factory instance.

```C#

IFactory<SomeClass> factory;
...
var instance = await factory.CreateAsync();
```

> You cannot generate a factory with a description that does not generate an instance (```FromInstance()``` ```BindPrefabAssetReference()```).
{style="note"}

To use a factory, inject ```IFactory<SomeClass>``` as follows.

```C#
public class SomeClass
{
    IFactory<SomeClass> factory;

    [Inject] async Task Construct(IFactory<SomeClass> factory)
    {
        this.factory = factory;　// Hold the factory
    }
    
    public async Task Create()
    {
        var instance = await factory.CreateAsync();　// Generate an instance at any time
    }
}
```

> Instances generated through a factory are generated separately for each injection target, just like ```AsTransient()```.
> Also, the generated instance may not be automatically destroyed when the DI container is released, so care is needed.
{style="note"}

You can use the binding syntax shown so far as is, so you can combine them freely to perform various bindings flexibly with simple coding.

Here are some examples.

### Factory that returns an interface

```C#
container.Bind<ISomeInterface>()
    .To<SomeClass>()
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<ISomeInterface> is bound.
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```


### Component factory

```C#
container.Bind<SomeComponent>()
    .On(someGameObject)
    .AsFactory();

// IFacoty<SomeComponent> is bound.
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```


### Prefab instance factory

```C#
container.BindPrefab<SomeComponent>(somePrefab)
    .Under(someTransform)
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<SomeComponent> is bound.
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```

### Addressable Asset prefab instance factory

```C#
container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
    .UnderSceneRoot()
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<SomeComponent> is bound.
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```