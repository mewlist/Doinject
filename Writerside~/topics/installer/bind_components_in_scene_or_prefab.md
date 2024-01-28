# シーンやプレハブに配置済みのコンポーネントをバインドする

インストーラーのインスペクタ内には、```Component Bindings``` という項目があります。
ここに、シーンに配置されたコンポーネントをドラッグ＆ドロップすることで、インストーラースクリプトを記述することなく、
そのコンポーネントをバインドできます。

![ComponentBindings](ComponentBindings.png)

```SomeComponent``` というコンポーネントを指定した場合、
DIコンテナには ```SomeComponent``` がバインドされ、コンテクスト空間内に注入可能となります。

## SerializeField を活用する

インストーラーの実態は、```MonoBehaviour``` もしくは ```ScriptableObject``` ですので、[SerializeField] プロパティを通じて、
他のオブジェクトを参照することができます。

Unity のオブジェクトを参照することで、それらをバインドでき、
プレハブの指定や、他のスクリプタブルオブジェクトのバインドができるようになっています。

```AssetReference``` や ```PrefabAssetReference``` を参照すれば ```Addressables``` で管理されたアセットをバインドすることもできます。

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
