# Doinject
Asynchronous DI Container for Unity

![Logo.svg](Writerside%7E/images/Logo.svg)

![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewAssets/blob/main/LICENSE)

* [日本語Readmeはこちら](https://github.com/mewlist/Doinject/blob/main/README_ja.md)

## Documentation 

[![Build documentation](https://github.com/mewlist/Doinject/actions/workflows/writerside.yml/badge.svg)](https://github.com/mewlist/Doinject/actions/workflows/writerside.yml)

* [日本語ドキュメント](https://mewlist.github.io/Doinject)
* English document is not available yet.

## Installation

### Install via Unity Package Manager

Install via Unity Package Manager
Please install the packages in the following order:

```
git@github.com:mewlist/MewCore.git
```

```
git@github.com:mewlist/Doinject.git
```

# About Doinject

Doinject is an asynchronous DI (Dependency Injection) framework for Unity.

The concept of asynchronous DI containers is the starting point.
Unfortunately, versions prior to Unity 2022 are not supported.

## Concepts

### Asynchronous DI Containers

The framework supports the creation and release of asynchronous instances.
This allows handling instances through the Addressables Asset Systems as well.
Moreover, you can delegate the creation of time-consuming instances to custom factories.

### Context Space Consistent with Unity's Lifecycle

Designed to define context spaces in a way that does not conflict with Unity's lifecycle.
When a scene is closed, the context associated with that scene is closed, the instances created in that context space disappear, and
destroying a GameObject with context similarly closes the context.
Context spaces are automatically structured by the framework, and parent-child relationships are formed when multiple contexts are loaded.


### Collaboration with the Addressable Asset System

Instances from the Addressable Asset System can also be handled, and the release of load handles can be automated.
Resource management in Addressables requires careful implementation, such as creating your own resource management system.
However, using Doinject automates the loading and release of Addressables.

### Simple coding

You can achieve replacements for the factory pattern, (context-closed) singleton pattern, and service locator pattern with simple descriptions.
Additionally, by creating custom factories or custom resolvers, you can handle more complex instance creation scenarios.


## Binding

### Type Binding

| Code                                                                  | Resolver behavior　                      | Type      |
|-----------------------------------------------------------------------|-----------------------------------------|-----------|
| ```container.Bind<SomeClass>();```                                    | ```new SomeClass()```                   | cached    |
| ```container.Bind<SomeClass>().AsSingleton();```　                     | ```new SomeClass()```                   | singleton |
| ```container.Bind<SomeClass>().AsTransient();```　                     | ```new SomeClass()```                   | transient |
| ```container.Bind<SomeClass>().Args(123,"ABC");```　                   | ```new SomeClass(123, "abc")```         | cached    |
| ```container.Bind<ISomeInterface>().To<SomeClass>();```　              | ```new SomeClass() as ISomeInterface``` | cached    |
| ```container.Bind<ISomeInterface, SomeClass>();```　                   | ```new SomeClass() as ISomeInterface``` | cached    |
| ```container.Bind<SomeClass>()```<br />```.FromInstance(instance);``` | ```instance```                          | instance  |
| ```container.BindInstance(instance);```                               | ```instance```                          | instance  |

### MonoBehaviour Binding

| Code                                                                  | Resolver behavior　                      |
|---------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| ```container.Bind<SomeComponent>();```                              | ```new GameObject().AddComponent<SomeComponent>()```                                                                    |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.Under(transform);``` | ```var instance = new GameObject().AddComponent<SomeComponent>();```<br/>```instance.transform.SetParent(transform);``` |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.On(gameObject);```   | ```gameObject.AddComponent<SomeComponent>()```                                                                             |
| ```container```<br />```.BindPrefab<SomeComponent>(somePrefab);```  | ```Instantiate(somePrefab).GetComponent<SomeComponent>()```                                                             |

### Addressables Binding


| Code                                                                  | Resolver behavior　                      |
|--------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ```container```<br />```.BindAssetReference<SomeAddressalbesObject>(assetReference);```    | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>(assetReference)```<br/><br/>```await handle.Task```　                                                                                    |
| ```container```<br />```.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);``` | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>(prefabAssetReference)```<br/><br/>```var prefab = await handle.Task```<br/><br/>```Instantiate(prefab).GetComponent<SomeComponent>()``` |
| ```container```<br />```.BindAssetRuntimeKey<SomeAddressalbesObject>("guid or path");```    | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>("guid or path")```<br/><br/>```await handle.Task```　                                                                                    |
| ```container```<br />```.BindPrefabAssetRuntimeKey<SomeComponent>("guid or path");```      | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>("guid or path")```<br/><br/>```var prefab = await handle.Task```<br/><br/>```Instantiate(prefab).GetComponent<SomeComponent>()```       |

### Factory Binding

| Code                                                                  | Resolver behavior　                      |
|-----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ```container```<br />```.Bind<SomeClass>()```<br />```.AsFactory();```                  | ```var resolver = new TypeResolver<SomeClass>()```<br/><br/>```new Factory<SomeClass>(resolver) as IFactory<SomeClass>```                                   |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.AsFactory();```              | ```var resolver = new MonoBehaviourResolver<SomeComponent>()```<br/><br/>```new Factory<SomeComponent>(resolver))```<br />``` as IFactory<SomeComponent>``` |
| ```container```<br />```.Bind<SomeClass>()```<br />```.AsCustomFactory<MyFactory>();``` | ```new CustomFactoryResolver<MyFactory>() as IFactory<SomeClass>```                                                                          |


## Injection

### Installer

```
public class SomeInstaller : BindingInstallerScriptableObject
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        container.Bind<SomeClass>();
    }
}
```

### Constructor Injection

```
class ExampleClass
{
    // Constructor Injection
    public ExampleClass(SomeClass someClass)
    { ... }
}
```

### Method Injection

```
class ExampleClass
{
    // Method Injection
    [Inject]
    public Construct(SomeClass someClass)
    { ... }
}
```

### Injection to MonoBehaviour

```
// Inherits IInjectableComponent
class ExampleComponent : MonoBehaviour, IInjectableComponent
{
    // Method Injection
    [Inject]
    public void Construct(SomeClass someClass)
    { ... }
}
```