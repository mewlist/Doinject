# Passing Arguments When Creating Context

When creating a context, you can pass arguments to it. These arguments are passed to the context's installer, and can be used to switch the features to be installed. Also, because they are registered in the DI container, they can be injected where needed.

> Unity scenes, in general, cannot change behavior based on given arguments, but by using context arguments, you can pass arguments to the scene when switching scenes.
{style="note"}

## How to Pass Arguments

By passing a type that inherits from `IContextArg` as the second argument to the `LoadAsync()` method of `SceneContextLoader` or `GameObjectContextLoader`, you can pass arguments to the context.

```C#
public class SomeContextArg : IContextArg
{
    public string SomeValue { get; set; }
}

// Passing arguments when creating a scene context
public class SomeLoader : MonoBehaviour, IInjectableComponent
{
    [SerializeField] SceneAssetReference nextScene;

    SceneContextLoader sceneContextLoader;
    
    [Inject]
    public void Construct(SceneContextLoader sceneContextLoader)
    {
        this.sceneContextLoader = sceneContextLoader;
    }
    
    public void LoadScene()
    {
        var arg = new SomeContextArg { SomeValue = "Hello" };
        await sceneContextLoader.LoadAsync(nextScene, active: true, arg);
    }
}
```

## How to Receive Arguments

The `Install()` method of the installer placed in the context to be loaded receives `IContextArg` as the second argument.

```C#

public class SomeInstaller : BindingInstallerComponent
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        
        // Switch the content to be installed using the argument passed to contextArg
        if (contextArg is SomeContextArg someContextArg)
        {
            // Switch the content to be installed using someContextArg.SomeValue
            container.Bind<SomeClass>()
                .Args(someContextArg.SomeValue);
                
        }
        else
        {
            container.Bind<SomeClass>()
                .Args("Default");
        }
    }
}

```