# Closing the Context Space

There are several ways to close a context space.
Choose the method that suits your situation.

## Call Dispose

By calling ```Dispose()``` on the context, you can close the context.
Objects belonging to their own context space are registered in the DI container as ```IContext```.

```C#
public IContext Context { get; set; }

[Inject]
public void Construct(IContext context)
{
    Context = context;
}

public async Task DisposeSceneContext()
{
    Context.Dispose();
}
```

## Scene Context Space: Call SceneContextLoader.UnloadAsync()

In the case of a scene context, you can close the context by holding the SceneContext when you load the scene.

```C#
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
```

## Unload with SceneManager

You can also close the scene context using the scene unload function provided by Unity.

```C#
SceneManager.UnloadSceneAsync(targetScene);
```

## GameObject Context Space: Call Destroy()

When you destroy an object with a GameObjectContext attached, the context is closed.

```C#
Destroy(gameObjectContext);
```
