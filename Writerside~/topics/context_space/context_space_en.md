# Context Space

The context space refers to the objects that the DI framework deals with, and the instances created by the DI container. The context space is defined for scenes and game objects.

![ContextSpace](ContextSpace.jpg){width=500}

## Scene Context Space

The entire scene is wrapped in a single large context. A scene context is created in the following cases:

* When the ```SceneContext``` component is placed in a scene, that scene is wrapped in a context.
* When another scene is loaded via the ```SceneContextLoader```, that scene creates a new context.

## GameObject Context Space

A context space wraps the entire hierarchy of children, starting from a certain game object. When treating a tree of game objects as a context, place the ```GameObjectContext``` at its root.

* Pre-place it in the scene
* Dynamically place it within the context space

In either case, a GameObject Context Space is created.

## Project Context Space

This is a context that wraps the entire project. If a project context is set, it becomes effective immediately after the application starts.

## Parent-Child Relationship of Contexts

When contexts have a parent-child relationship, child contexts can refer to the DI container belonging to the parent context. Types and instances registered in the parent context can be referred to from the child context, but not vice versa, so be careful.

By creating a scene context that covers the entire application, binding objects that require global access in design, achieved by the service locator pattern or singleton pattern, and making the minimum necessary binds in the child context, the context boundaries can be clarified.

## Context Lifecycle

The context space can be closed at any time. If it's a scene context, it's released when the scene is closed. If it's a GameObject context, it's released when that GameObject is destroyed. By calling `SceneContext.Dispose()` or `GameObjectContext.Dispose()`, you can explicitly close the context.

If there are instances belonging to the closed context space, those instances are automatically destroyed. However, be careful as instances created via a factory or instances bound with ```AsTransient``` may not be destroyed.

> In the case of scene context, closing a scene means that all instances hanging in the hierarchy of that scene are also destroyed. In most cases, this is not a problem, but if you move an instance to another scene or outside the hierarchy of the GameObject context, you need to destroy that instance yourself.
{style="note"}

Doinject is designed with the concept of having a context space that does not contradict the lifecycle of Unity.

## Injection into Components Pre-Placed in Scene or GameObject Context Children

In order to inject dependencies into components that are pre-placed under the scene or GameObject context, components inheriting ```MonoBehaviour``` need to inherit the ```IInjectableComponent``` interface.

```C#
public class SomeComponent : MonoBehaviour, IInjectableComponent // Inherit IInjectableComponent
{
    [Inject] // Add [Inject] attribute
    public void Construct(SomeDependency dependency)
    {
        ...
    }
}
```

> We have this restriction because it is costly to investigate all components of a scene.
{style="note"}