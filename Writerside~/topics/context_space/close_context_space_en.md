# Closing the Context Space

There are several ways to close the context space.
Please choose the appropriate method according to your situation.

## Calling Dispose

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

## Scene Context Space: Calling SceneContextLoader.UnloadAsync()

In the case of scene context, by keeping the SceneContext when loading the scene, you can close that context.

```C#
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
```

> Even when the scene is closed directly by methods other than this, the scene context itself is automatically closed, but
> when unloading a scene loaded with Addressables, the handle will not be released, so always use the scene loader or dispose of the context.
{style="warning"}


## GameObject Context Space: Calling Destroy()

When an object with a GameObject context attached is destroyed, that context is closed.

```C#
Destroy(gameObjectContext);
```
