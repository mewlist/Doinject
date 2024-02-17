# Switching Scenes

To switch scenes, you unload the loaded scene context and then load a new scene. Scenes loaded as child contexts can be unloaded all at once by calling ```SceneContextLoader.UnloadAllScenesAsync()```.

```C#
await sceneContextLoader.UnloadAllAsync();
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

To unload a specific scene, keep the return value of ```LoadAsync()``` and call ```UnloadAsync()``` at the necessary timing.

```C#
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

## Closing Your Own Scene Context Space and Opening Another Scene Context

To close the scene context you belong to, go through ```IContext.OwnerSceneContextLoader```.

> Please note that ```OwnerSceneContextLoader``` is not provided for the ```Project Context``` itself or the entry point context of a project without a set ```Project Context```.
{style="note"}

```C#

[SerializeField] public SceneAssetReference nextSceneAssetReference;

public IContext Context { get; set; }

[Inject]
void Constrruct(IContext context)
{
    Context = context;
}

public void LoadNextScene()
{
    await Context.OwnerSceneContextLoader.UnloadAllScenesAsync();
    await Context.OwnerSceneContextLoader.LoadAsync(nextSceneAssetReference, active: true);
}
```

> This is just an introduction to what you can do, and it's debatable whether this coding style can be recommended. It would be best to prepare a scene management class that incorporates the coding introduced here and is suitable for the application's mechanism. By binding the scene management class to the context and switching scenes through the scene management function from the child context, you can improve the visibility.
