# シーンコンテクスト空間

シーン全体を一つの大きなコンテクストで包みます。シーンコンテクスト空間が生成されるタイミングのは以下の場合です。

* ```ContextEntryPoint``` コンポーネントをシーンに配置した場合、そのシーンはコンテクストに包まれます。
* ```SceneContextLoader``` を経由して別のシーンをロードすると、そのシーンは、現在のコンテクストの子として新たなコンテクスト空間を生成します。

## ```AutoContextLoader``` を使ってロードする

コンテクスト空間内の任意の場所に、```AutoContextLoader``` コンポーネントを配置することで、
そのコンテクスト空間の子となるシーンコンテクストやゲームオブジェクトコンテクストをロードできます。

インスペクタの <control>Scene Contexts</control> にロードしたいシーンを指定することで、
```AutoContextLoader```の属するコンテクスト空間がロードされた後、自動的に子コンテクストがロードされます。

![AutoContextLoader.png](AutoContextLoader.png)


## ```SceneContextLoader``` を使ってシーンをロードする

各コンテクスト空間に属する DIコンテナには、必ず `SceneContextLoader` が提供されます。
```SceneContextLoader.LoadAsync()``` にて別のシーンをロードすることで、現在のコンテクストの子となるシーンコンテクストを生成することができます。
```SceneContextLoader``` は、DIコンテナに自動的に登録されるため、 必要な場所で注入してもらいます。

子コンテクストとなるシーンを、ロードするコンポーネントを作ってみましょう。

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
    
    public async Task OnInjected()
    {
        await sceneContextLoader.LoadAsync(childScene, active: true);
    }
}
```

このコンポーネントを、シーンに配置し、インスペクタより ```childScene``` にシーンを指定することで、
そのシーンをロードできます。

> ### UnifiedScene 型について
> 
> `SceneContextLoader.LoadAsync` の第一引数には、`UnifiedScene` 型を指定します。
> `UnifiedScene` は、BuildSettings を経由したシーンと、Addressables を経由したシーンの
> 複数のシーン指定方法を透過的に扱うために用意されています。
> 
> SerializeField　に UnifiedScene を指定すると、インスペクタで以下のように表示されます。
> (Addressables パッケージがプロジェクトに導入されている場合)
> 
> ![UnifiedScene](UnifiedScene.png)
> 
> BuildSettings にて指定されているシーン(```SceneReference```)か、
> Addressables に登録されているシーン(```SceneAssetReference```)のいずれかを指定することで、
> 異なるローディング方法のシーンを透過的に扱うことができます。
> 二つとも指定されている場合は、```SceneAssetReference``` が優先されます。
> 
> Addressables パッケージを導入していないプロジェクトの場合は、SceneReference のみ表示されます。
>
> Addressables 管理のシーンしか扱わないプロジェクトの場合は、SceneAssetReference を直接使っても問題ありません。
> 同様に BuildSettings で指定されたシーンしか扱わない場合は、SceneReference を使ってもシーンのロードが可能です。
> しかしながら、基本的には UnifiedScene を使うことをおすすめします。
{style="note"}


## 動作確認

```LoadChildScene``` を EntryPoint となるシーンに配置したら、実行してみましょう。
EntryPoint となるシーンがロードされると、子シーンがロードされることが確認できます。

DI Context Tree を確認すると、親子関係を持ったシーンコンテクストが生成されていることが確認できます。
