# Scene Context Space

The entire scene is wrapped in one large context. The following are the cases when a scene context space is generated.

* If a ```SceneContext``` component is placed in the scene, that scene is wrapped in the context.
* When loading another scene via ```SceneContextLoader```, that scene generates a new context space as a child of the current context.
 
> Even scenes loaded using ```SceneContextLoader``` do not need a ```SceneContext``` component. A context space is automatically generated.
{style="note"}

## Loading with ```AutoContextLoader```

By placing the ```AutoContextLoader``` component anywhere within the context space, you can load scene contexts and game object contexts that are children of that context space.

By specifying the scene you want to load in the inspector's <control>Scene Contexts</control>, after the context space where ```AutoContextLoader``` belongs is loaded, the child context is automatically loaded.

![AutoContextLoader.png](AutoContextLoader.png)


## Loading a Scene with ```SceneContextLoader```

In each context space, a `SceneContextLoader` is always provided to the DI container belonging to it. By loading another scene with ```SceneContextLoader.LoadAsync()```, you can generate a scene context that is a child of the current context. The ```SceneContextLoader``` is automatically registered in the DI container, so it will be injected wherever needed.

Let's create a component to load a scene that will become a child context.

```C#
public class LoadChildScene : MonoBehaviour, IInjectableComponent
{
    [SerializeField] UnifiedScene childScene;

    SceneContextLoader sceneContextLoader;

    [Inject]
    public async Task Construct(SceneContextLoader sceneContextLoader)
    {
        this.sceneContextLoader = sceneContextLoader;
    }
    
    [OnInjected]
    public async Task OnInjected()
    {
        await sceneContextLoader.LoadAsync(childScene, active: true);
    }
}
```

By placing this component in the scene and specifying a scene in ```childScene``` from the inspector, you can load that scene.

> ### About UnifiedScene Type
> 
> The first argument of `SceneContextLoader.LoadAsync` specifies the `UnifiedScene` type.
> `UnifiedScene` is prepared to transparently handle multiple scene specification methods through BuildSettings and scenes via Addressables.
> 
> When you specify UnifiedScene in SerializeField, it is displayed as follows in the inspector.
> (When the Addressables package is introduced into the project)
> 
> ![UnifiedScene](UnifiedScene.png)
> 
> You can transparently handle scenes with different loading methods by specifying either a scene specified in BuildSettings (```SceneReference```) or a scene registered in Addressables (```SceneAssetReference```).
> If both are specified, ```SceneAssetReference``` takes precedence.
> 
> For projects that do not have the Addressables package installed, only SceneReference is displayed.
>
> For projects that only handle scenes under Addressables management, there is no problem using SceneAssetReference directly.
> Similarly, if you only handle scenes specified in BuildSettings, you can load scenes using SceneReference.
> However, it is generally recommended to use UnifiedScene.
{style="note"}


## Operation Confirmation

Once you've placed ```LoadChildScene``` in a scene that will be the EntryPoint, try running it.
When the EntryPoint scene is loaded, you can confirm that the child scene is loaded.

By checking the DI Context Tree, you can confirm that scene contexts with parent-child relationships have been generated.