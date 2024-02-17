# GameObject Context Space

A GameObject Context Space wraps the entire hierarchy of children, starting from a particular game object. When treating a tree of game objects as a context, you place the ```GameObjectContext``` component on the root object.

There are several ways to generate a GameObject Context Space:

* Directly place it in the scene
* Create a prefab and instantiate it within the context space

## Directly Place in the Scene

Place the game object in the scene that will become the context space as follows:

![GameObjectContext](GameObjectContextTree.png)

In this example, the node named GameObjectContext becomes the root of the context space. All child objects are included in the context space. You can freely place installers and necessary components, just like in a scene context.

First, attach the GameObjectContext component to the root.

![GameObjectComponent](GameObjectContextComponent.png)

With this, the setting of the GameObject Context is complete. When you check the operation, you can confirm that the state of the context tree is as follows:

![GameObjectContext ContextTree](GameObjectContextContextTree.png)

You can confirm that the GameObject Context is generated as a child of the Scene Context. You can also confirm that bindings are made according to the definitions of the installers placed in the GameObject Context.

## Create a Prefab and Instantiate within the Context Space

Prefab the GameObject Context you just created, and instantiate the prefab after the context is initialized. To instantiate the prefab that becomes the GameObject Context, use ```GameObjectContextLoader.LoadAsync()```. ```GameObjectContextLoader``` is automatically bound to the DI container, just like ```SceneContextLoader```.

```C#

[SerializeField] GameObject gameObjectContextPrefab;

public class SomeComponent : MonoBehaviour, IInjectableComponent
IContext context;

[Inject]
public void Construct(IContext context)
{
    this.context = context;
}

// OnInjected method is implicitly called after all dependencies are injected if defined.
[OnInjected]
public async Task OnInjected()
{
    await context.GameObjectContextLoader.LoadAsync(gameObjectContextPrefab);
}
```

## Load using ```AutoContextLoader```

You can load the prefabbed GameObject Context by placing the ```AutoContextLoader``` component anywhere within the context space.

After the context space where ```AutoContextLoader``` belongs is loaded, the child context is automatically loaded by specifying the prefab you want to load in the inspector's <control>Game Object Context Prefabs</control>.

![AutoContextLoader.png](AutoContextLoader.png)


## Lifecycle of GameObject Context

By prefabbing this GameObject Context and dragging and dropping it into the Scene Context during playback, you can confirm in the DI Context Tree Window that the GameObject Context is generated in real time.

Also, if you delete the context root in the hierarchy view, you can confirm in the DI Context Tree Window that the context is destroyed in real time.

In this way, the GameObject Context is designed to be easy to debug during development without contradicting Unity's lifecycle.

When you want to check the behavior of a UI that functions as a dialog, or when you are developing a module with a small context, the GameObject Context is useful.