# コンテクストの生成時に引数を渡す

コンテクストを生成する際に、コンテクストに対して引数を渡せます。
引数は、コンテクストのインストーラーに渡され、インストールする機能を切り替えるといった活用ができます。
また、DIコンテナに登録されるため、必要な場所へ注入することもできます。

> Unity のシーンは、引数を与えて挙動を切り替えるといったことは基本的にできませんが、
> コンテクスト引数を使うことによって、シーンを切り替える際に、シーンに対して引数を渡すといった使い方ができます。
{style="note"}

## 引数の渡し方

```SceneContextLoader``` もしくは、```GameObjectContextLoader``` の ```LoadAsync()```　メソッドの第二引数に、
```IContextArg``` を継承した型を渡すことで、コンテクストに対して引数を渡すことができます。

```C#
public class SomeContextArg : IContextArg
{
    public string SomeValue { get; set; }
}

// シーンコンテクストを生成時に引数を渡す
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

## 引数の受け取り方

ロードされるコンテクストに配置されたインストーラーの ```Install()``` メソッドの第二引数に、```IContextArg``` が渡されます。

```C#

public class SomeInstaller : BindingInstallerComponent
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        
        // contextArg に渡された引数を使って、インストール内容を切り替える
        if (contextArg is SomeContextArg someContextArg)
        {
            // someContextArg.SomeValue を使ってインストール内容を切り替える
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