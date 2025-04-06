# Doinject
Asynchronous DI Container for Unity

![Logo.svg](Writerside%7E/images/Logo.svg)

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewAssets/blob/main/LICENSE)

* [日本語Readmeはこちら](https://github.com/mewlist/Doinject/blob/main/README_ja.md)

## Table of Contents

- [Documentation](#documentation)
- [Example Project](#example-project)
- [Installation](#installation)
  - [Install via Unity Package Manager](#install-via-unity-package-manager)
- [About Doinject](#about-doinject)
  - [Concepts](#concepts)
    - [Asynchronous DI Containers](#asynchronous-di-containers)
    - [Context Scopes Aligned with Unity's Lifecycle](#context-scopes-aligned-with-unitys-lifecycle)
    - [Integration with the Addressable Asset System](#integration-with-the-addressable-asset-system)
    - [Simplified Dependency Patterns](#simplified-dependency-patterns)
  - [Binding](#binding)
    - [Type Binding](#type-binding)
    - [MonoBehaviour Binding](#monobehaviour-binding)
    - [Addressables Binding](#addressables-binding)
    - [Factory Binding](#factory-binding)
  - [Injection](#injection)
    - [Installer](#installer)
    - [Constructor Injection](#constructor-injection)
    - [Method Injection](#method-injection)
    - [Injection into MonoBehaviours](#injection-into-monobehaviours)

## Documentation

[![Build documentation](https://github.com/mewlist/Doinject/actions/workflows/writerside.yml/badge.svg)](https://github.com/mewlist/Doinject/actions/workflows/writerside.yml)

* [日本語ドキュメント](https://mewlist.github.io/Doinject)
* [Documents in English](https://mewlist.github.io/Doinject/en/introduction-en.html)

## Example Project

* [Example Project](https://github.com/mewlist/DoinjectExample)


## Installation

### Install via Unity Package Manager

Please install the packages in the following order:

```
https://github.com/mewlist/MewCore.git
```

```
https://github.com/mewlist/Doinject.git
```

# About Doinject

Doinject is an asynchronous Dependency Injection (DI) framework for Unity.

It is built around the concept of an asynchronous DI container.
Supports Unity 2022.3 LTS / Unity 6 and later.

## Concepts

### Asynchronous DI Containers

Traditional DI containers typically create instances synchronously.
However, this approach doesn't easily support Unity's asynchronous operations, such as loading assets or fetching data before an object can be fully initialized.

Doinject provides a DI container designed for asynchronous workflows. It supports creating and disposing of instances through asynchronous processes.
This allows for more flexible dependency management with a clear API, enabling scenarios like:
*   Instantiating objects after asynchronously loading their prefabs or assets via the Addressables system.
*   Initializing services with data fetched asynchronously from a network or file.
Custom factories can be created to handle any asynchronous instantiation logic.

### Context Scopes Aligned with Unity's Lifecycle

Doinject defines context scopes that naturally align with Unity's object lifecycle:
*   **Project Context:** Lives for the entire application duration.
*   **Scene Context:** Tied to a specific scene's lifetime. When the scene is unloaded, the context and its associated instances are disposed.
*   **GameObject Context:** Tied to a specific GameObject's lifetime. When the GameObject is destroyed, the context and its instances are disposed.
These contexts automatically form parent-child relationships (e.g., Scene Context inherits from Project Context), allowing dependencies to be resolved hierarchically.

### Integration with the Addressable Asset System

Doinject seamlessly integrates with Unity's Addressable Asset System.
You can bind Addressable assets directly, and Doinject will automatically handle loading the asset asynchronously when needed and releasing its handle when the associated context is disposed. This simplifies resource management significantly compared to manual handle tracking.

### Simplified Dependency Patterns

Doinject simplifies the implementation of common dependency management patterns:
*   **Factory Pattern:** Easily bind factories for creating instances on demand.
*   **Singleton Pattern:** Bind objects as singletons scoped to their context (Project, Scene, or GameObject).
*   **Service Locator:** While DI is generally preferred, Doinject can be used to manage globally or locally accessible services.
Custom factories and resolvers offer further flexibility for complex instantiation logic.


## Binding

### Type Binding

| Code                                                                  | Resolver Behavior                       | Type      |
|-----------------------------------------------------------------------|-----------------------------------------|-----------|
| ```container.Bind<SomeClass>();```                                    | ```new SomeClass()```                   | cached    |
| ```container.Bind<SomeClass>().AsSingleton();```                     | ```new SomeClass()```                   | singleton |
| ```container.Bind<SomeClass>().AsTransient();```                     | ```new SomeClass()```                   | transient |
| ```container.Bind<SomeClass>().Args(123,"ABC");```                   | ```new SomeClass(123, "abc")```         | cached    |
| ```container.Bind<ISomeInterface>().To<SomeClass>();```              | ```new SomeClass() as ISomeInterface``` | cached    |
| ```container.Bind<ISomeInterface, SomeClass>();```                   | ```new SomeClass() as ISomeInterface``` | cached    |
| ```container.Bind<SomeClass>()```<br />```.FromInstance(instance);``` | ```instance```                          | instance  |
| ```container.BindInstance(instance);```                               | ```instance```                          | instance  |

**Binding Lifetimes:**

*   **`cached` (Default):** Creates an instance on the first resolution within its container and reuses that same instance for subsequent requests within that container. The instance is disposed (if `IDisposable` or `IAsyncDisposable`) when the container is disposed.
*   **`singleton`:** Creates a single instance within the context scope where it was first resolved. This instance persists for the lifetime of that context and is reused for all requests within that context and its child contexts. It is disposed when the context is disposed.
*   **`transient`:** Creates a new instance every time it is resolved. Doinject does not manage the lifecycle (creation/disposal) of transient instances beyond initial creation; manual disposal might be necessary.
*   **`instance`:** Binds a pre-existing instance to the container. Doinject does not manage the lifecycle (creation/disposal) of this instance.

### MonoBehaviour Binding

| Code                                                                  | Resolver Behavior                       |
|---------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| ```container.Bind<SomeComponent>();```                              | ```new GameObject().AddComponent<SomeComponent>()```                                                                    |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.Under(transform);``` | ```var instance = new GameObject().AddComponent<SomeComponent>();```<br/>```instance.transform.SetParent(transform);``` |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.On(gameObject);```   | ```gameObject.AddComponent<SomeComponent>()```                                                                             |
| ```container```<br />```.BindPrefab<SomeComponent>(somePrefab);```  | ```Instantiate(somePrefab).GetComponent<SomeComponent>()```                                                             |

### Addressables Binding

| Code                                                                  | Resolver Behavior                       |
|--------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ```container```<br />```.BindAssetReference<SomeAddressableObject>(assetReference);```    | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>(assetReference)```<br/><br/>```await handle.Task```                                                                                       |
| ```container```<br />```.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);``` | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>(prefabAssetReference)```<br/><br/>```var prefab = await handle.Task```<br/><br/>```Instantiate(prefab).GetComponent<SomeComponent>()``` |
| ```container```<br />```.BindAssetRuntimeKey<SomeAddressableObject>("guid or path");```    | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>("guid or path")```<br/><br/>```await handle.Task```　                                                                                    |
| ```container```<br />```.BindPrefabAssetRuntimeKey<SomeComponent>("guid or path");```      | ```var handle = Addressables```<br />```.LoadAssetAsync<GameObject>("guid or path")```<br/><br/>```var prefab = await handle.Task```<br/><br/>```Instantiate(prefab).GetComponent<SomeComponent>()```       |

### Factory Binding

| Code                                                                  | Resolver Behavior                       |
|-----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ```container```<br />```.Bind<SomeClass>()```<br />```.AsFactory();```                  | ```var resolver = new TypeResolver<SomeClass>()```<br/><br/>```new Factory<SomeClass>(resolver) as IFactory<SomeClass>```                                   |
| ```container```<br />```.Bind<SomeComponent>()```<br />```.AsFactory();```              | ```var resolver = new MonoBehaviourResolver<SomeComponent>()```<br/><br/>```new Factory<SomeComponent>(resolver))```<br />``` as IFactory<SomeComponent>``` |
| ```container```<br />```.Bind<SomeClass>()```<br />```.AsCustomFactory<MyFactory>();``` | ```new CustomFactoryResolver<MyFactory>() as IFactory<SomeClass>```                                                                          |


Factory bindings can also be combined with Addressables.
For example, you can create a factory that asynchronously loads a prefab via Addressables, instantiates it, and returns a specific component:
```cs
container
  .BindAssetReference<SomeComponentOnAddressalbesPrefab>(assetReference)
  .AsFactory<SomeComponentOnAddressalbesPrefab>();
```
```cs
[Inject]
void Construct(IFactory<SomeComponentOnAddressalbesPrefab> factory)
{
  var instance = await factory.CreateAsync();
}
```

## Injection

### Installer

```cs
public class SomeInstaller : BindingInstallerScriptableObject
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        container.Bind<SomeClass>();
    }
}
```

### Constructor Injection

```cs
class ExampleClass
{
    // Constructor Injection
    public ExampleClass(SomeClass someClass)
    { ... }
}
```

### Method Injection

```cs
class ExampleClass
{
    // Method Injection
    [Inject]
    public Construct(SomeClass someClass)
    { ... }
}
```

### Injection into MonoBehaviours

To enable injection into a `MonoBehaviour`, it must implement the `IInjectableComponent` interface. Dependencies can then be injected via constructor (if applicable) or method injection.

```cs
using UnityEngine;
using Doinject;

// Inherit from MonoBehaviour and implement IInjectableComponent
public class ExampleComponent : MonoBehaviour, IInjectableComponent
{
    private SomeClass _someClassDependency;

    // Method Injection using [Inject] attribute
    [Inject]
    public void Construct(SomeClass someClass)
    {
        _someClassDependency = someClass;
        // ... use the dependency
    }
}
```
The container automatically finds and calls methods marked with `[Inject]` on components that implement `IInjectableComponent` within its context scope after the component is enabled.
