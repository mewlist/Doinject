# Binding Components Already Placed in Scenes or Prefabs

In the inspector of the installer, there is a section called ```Component Bindings```.
By dragging and dropping the components deployed in the scene into this section, you can bind the components without having to code the installer script.

![ComponentBindings](ComponentBindings.png)

If you specify a component called ```SomeComponent```, it will be bound to the DI container and can be injected into the context space.

## Utilizing SerializeField

Since the actual installer is either ```MonoBehaviour``` or ```ScriptableObject```, you can refer to other objects through the [SerializeField] property.

By referring to Unity objects, they can be bound, and it is now possible to specify prefabs or bind other scriptable objects.

You can also bind assets managed by ```Addressables``` by referring to ```AssetReference``` or ```PrefabAssetReference```.

```C#
public class SomeInstaller : BindingInstallerComponent
{
    [SerializeField] PrefabAssetReference prefabAssetReference;

    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        // Bind your dependencies here
        container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);
    }
}
```