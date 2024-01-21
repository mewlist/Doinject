# コンテクスト空間

コンテクスト空間とは、DIフレームワークが扱う対象となるオブジェクトや、DIコンテナによって生成されたインスタンスを括ったものです。
コンテクスト空間は、シーンやゲームオブジェクトに対して定義されます。

![ContextSpace](../images/ContextSpace.jpg){width=500px}

## シーンコンテクスト空間

シーン全体を一つの大きなコンテクストで包みます。シーンコンテクストが生成されるのは以下の場合です。

* ```ContextEntryPoint``` コンポーネントをシーンに配置した場合、そのシーンはコンテクストに包まれます。
* ```SceneContextLoader``` を経由して別のシーンをロードすると、そのシーンは新たなコンテクストを生成します。


## ゲームオブジェクトコンテクスト空間

あるゲームオブジェクトを起点として、ヒエラルキ上の子全体をコンテクスト空間に包み込みます。
ゲームオブジェクトのツリーをコンテクストとして扱う場合は、```GameObjectContext``` をそのルートに配置します。

* あらかじめシーンに配置しておく
* コンテクスト空間内に動的に配置する

いずれのケースでもゲームオブジェクトコンテクスト空間が生成されます。

## シーンや、ゲームオブジェクトコンテクストの子として、最初から配置されているオブジェクトへの注入

シーンやゲームオブジェクトコンテクスト以下にあらかじめ配置されているコンポーネントへ依存を注入するためには、
```MonoBehaviour``` を継承したコンポーネントが ```IInjectableComponent``` インターフェースを継承している必要があります。

```csharp
public class SomeComponent : MonoBehaviour, IInjectableComponent // IInjectableComponent を継承する
{
    [Inject] // [Inject] 属性をつける
    public void Construct(SomeDependency dependency)
    {
        ...
    }
}
```

> [!NOTE]
> シーンのコンポーネントすべてを調査するのはコストがかかるため、このような制約をおいています。


## DI Context Tree Window でコンテクスト空間の構造を確認する

ランタイムでコンテクスト空間がどのような構造を持っているか、また、各コンテクストに属する DI コンテナの状態を確認することができます。

Unity のメニューから、```Window > Doinject > DI Context Tree``` を選択してください。
以下のような Window が表示され、コンテクストの種類と親子関係を確認できます。

![DI Context Tree](..//images/DIContextTree_Bindings.png)

それぞれのコンテクストを選択すると、そのコンテクストに属する DI コンテナの状態を確認できます。

Bindinds タブでは、DI コンテナに登録されている型とインスタンスの提供方法を確認できます。

Instances タブでは、DI コンテナによって生成されたインスタンスを確認できます。AsTransient や、Factory で生成されたインスタンスは、
DIコンテナの管理から外れるためここでは表示されません。

## コンテクストの親子関係

コンテクストが親子関係を持つ場合、子コンテクストは親コンテクストに属する DI コンテナを参照することができます。
親コンテクストに登録された型やインスタンスは、子コンテクストから参照することができますが、その逆はできないので注意してください。

Application 全体を覆うシーンコンテクストを作成し、サービスロケーターパターンやシングルトンパターンで実現されていた、設計上グローバルなアクセスを必要とするオブジェクトをバインドし、
子のコンテクストでは、そこで必要とする最小限のバインドをする、といった構成をとることで、コンテクスト境界を明確にすることができます。

## コンテクストのライフサイクル

コンテクスト空間は好きなタイミングで閉じることができます。
シーンコンテクストであればシーンを閉じたタイミングで、ゲームオブジェクトコンテクストであればそのゲームオブジェクトを破棄したタイミングで解放されます。
`SceneContext.Dispose()` `GameObjectContext.Dispose()` を呼び出すことで、明示的にコンテクストを閉じることもできます。

もし、閉じたコンテクスト空間に属するインスタンスがあった場合、そのインスタンスは自動的に破棄されます。
ただし、ファクトリ経由で生成したインスタンスや ```AsTransient``` でバインドされたインスタンスは、破棄されない可能性があるので注意してください。

>[!NOTE]
> シーンコンテクストの場合、シーンを閉じるということは、そのシーンのヒエラルキにぶら下がるインスタンスもすべて破棄されるということですので、
> ほとんどのケースでは問題がありませんが、別のシーンにインスタンスを移動させたり、ゲームオブジェクトコンテクストのヒエラルキの外側にインスタンスを移動させたりする場合は、
> そのインスタンスの破棄は自分で行う必要があります。

Doinject はこのように、Unity のライフサイクルと矛盾しないコンテクスト空間を持つことをコンセプトとして設計されています。

## 子コンテクストの生成 - シーンコンテクスト

シーンコンテクスト、ゲームオブジェクトコンテクストを起点に、子となるコンテクストを生成することができます。

### シーンロードを経由して子コンテクストを生成する

各コンテクスト空間に属する DIコンテナには、必ず `SceneContextLoader` が提供されます。
`SceneContextLoader` を経由して、別のシーンをロードすることで、子どもとなるシーンコンテクストを生成することができます。
`SceneContextLoader` をインジェクトしてインスタンスを獲得し、`LoadAsync` を呼び出すことで、子シーンをロードすることができます。

子コンテクストとなるシーンを、ロードするコンポーネントを作ってみましょう。

```csharp
public class LoadChildScene : MonoBehaviour, IInjectableComponent
{
    [SerializeField] UnifiedScene childScene;

    [Inject]
    public async Task Construct(SceneContextLoader sceneContextLoader)
    {
        await sceneContextLoader.LoadAsync(childScene, active: true);
    }
}
```

このコンポーネントを、シーンに配置し、childScene に利用可能なシーンを指定することで、
そのシーンをロードすることができます。

#### UnifiedScene について

`SceneContextLoader.LoadAsync` の第一引数には、`UnifiedScene` 型を指定します。
`UnifiedScene` は、BuildSettings を経由したシーンと、Addressalbes を経由したシーンの
複数のシーン指定方法を透過的に扱うために用意されています。

SerializeField　に UnifiedScene を指定すると、インスペクタで以下のように表示されます。
(Addressables パッケージがプロジェクトに導入されている場合)

![UnifiedScene](../images/UnifiedScene.png)

BuildSettings にて指定されているシーン(```SceneReference```)か、
Addressables に登録されているシーン(```SceneAssetReference```)のいずれかを指定することで、
異なるローディング方法のシーンを透過的に扱うことができます。
二つとも指定されている場合は、```SceneAssetReference``` が優先されます。

>[!NOTE]
> Addressables パッケージを導入していないプロジェクトの場合は、SceneReference のみ表示されます。

>[!NOTE]
> Addressables 管理のシーンしか扱わないプロジェクトの場合は、SceneAssetReference を直接使っても問題ありません。
> 同様に BuildSettings で指定されたシーンしか扱わない場合は、SceneReference を使ってもシーンのロードが可能です。
> しかしながら、基本的には UnifiedScene を使うことをおすすめします。

#### 動作確認

```LoadChildScene``` を EntryPoint となるシーンに配置したら、実行してみましょう。
EntryPoint となるシーンがロードされると、子シーンがロードされることが確認できます。

DI Context Tree を確認すると、親子関係を持ったシーンコンテクストが生成されていることが確認できます。

## 子コンテクストの生成 - ゲームオブジェクトコンテクスト

次にゲームオブジェクトコンテクストを子のコンテクストとして動作させてみましょう。

### シーンに配置する場合


エントリポイントとなるシーンに以下のようにゲームオブジェクトを配置します。
![GameObjectContext](../images/GameObjectContextTree.png)

この例では、GameObjectContext という名前のノードがコンテクスト空間のルートとなり、子オブジェクトはすべてコンテクスト空間に包まれ、
シーンコンテクストと同様に、インストーラーや必要なコンポーネントを自由に配置することができます。

まずは、ルートに GameObjectContext コンポーネントをアタッチします。

![GameObjectComponent](../images/GameObjectContextComponent.png)

以上で、ゲームオブジェクトコンテクストの設定は完了です。
動作を確認すると、コンテクストツリーの状態が以下のようになっていることが確認できます。

![GameObjectContext ContextTree](../images/GameObjectContextContextTree.png)

シーンコンテクストの子として、ゲームオブジェクトコンテクストが生成されていることが確認できます。
また、ゲームオブジェクトコンテクスト内に配置したインストーラの定義に従ったバインディングが行われていることも確認できます。

### 動的に生成する場合

以下のように、コンテクストが初期化されたあとに、ゲームオブジェクトコンテクストが設定されたプレハブをロードします。

```csharp

[SerializeField] GameObject gameObjectContextPrefab;

public class SomeComponent : MonoBehaviour, IInjectableComponent
IContext context;

[Inject]
public void Construct(IContext context)
{
    this.context = context;
}

// OnInjected method is implicitly called after all dependencies are injected if defined.
public void OnInjected()
{
    context.GameObjectContextLoader.LoadAsync(gameObjectContextPrefab, gameObjectContextPrefab);
}
```

```GameObject.Instantiate()``` で生成しても良いですが、マルチシーン運用時には呼び出したシーンにインスタンスが生成されない場合があります。
そのため、GameObjectContextLoader を経由して生成することを推奨します。

### ゲームオブジェクトコンテクストのライフサイクル

このゲームオブジェクトコンテクストをプレハブ化し、再生中のシーンコンテクストにドラッグ＆ドロップすることで、
ゲームオブジェクトコンテクストがリアルタイムに生成されることが DI Context Tree Window で確認できます。

また、ヒエラルキビュー上でコンテクストルートを削除すると、
リアルタイムにコンテクストが破棄されることが、同様に DI Context Tree Window で確認できます。

このように、ゲームオブジェクトコンテクストは、Unity のライフサイクルと矛盾せず、開発中のデバッグがしやすいように設計されています。

ダイアログとして機能するUIの挙動を確認したいときなど、小さなコンテクストを持つモジュールを開発するとき、ゲームオブジェクトコンテクストは有用です。

## コンテクストを閉じる

コンテクストに対して Dispose を呼び出すことで、コンテクストを閉じることができます。

```csharp
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

シーンコンテクストの場合は、シーンをロードした際に、SceneContext を保持しておくことで、そのコンテクストを閉じることができます。

```csharp
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
```

> [!NOTE]
> これ以外の方法で直接シーンを閉じたときでも、シーンコンテクスト自体は自動的に閉じられますが、
> Addressables でロードしたシーンをアンロードする場合は、そのハンドルが解放されなくなってしまうので、必ず、シーンローダーを使うか、コンテクストの破棄をするようにしてください。


ゲームオブジェクトコンテクストがアタッチされたオブジェクトを破棄すると、そのコンテクストは閉じます。

```csharp
Destroy(gameObjectContext);
```


## シーンを切り替える

シーンを切り替えるには、エントリポイントとなるシーンのコンテクストで、```SceneContextLoader``` をインジェクトして、
```UnloadAllScenesAsync()``` ```LoadAsync()``` を呼び出します。

```csharp
await sceneContextLoader.UnloadAllAsync();
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

特定のシーンをアンロードするには、```LoadAsync()``` の戻り値を保持しておき、
必要なタイミングで ```UnloadAsync()``` を呼び出します。

```csharp
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

### 自身のシーンコンテクストを破棄して、別のシーンを呼び出す

```csharp

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

自身の属するコンテクストを取得し、そのコンテクストの ```OwnerSceneContextLoader``` を経由して、
シーンを切り替えることができます。
エントリポイントコンテクストには、```OwnerSceneContextLoader``` が提供されていないので注意してください。


>[!TIP]
> 一応、こういうこともできるという紹介で、この書き方が推奨できるかは微妙です。
> ここで紹介した記述を組み込んだ、アプリケーションの機構に適したにシーン管理クラスを用意するのが良いでしょう。
> シーン管理クラスをコンテクストにバインドして、子のコンテクストからも、シーン管理機能を通してシーンを切り替えるようにすると見通しが良くなります。

## 補足

> [!IMPORTANT]
> Doinject では、プロジェクト全体を表現するコンテクスト空間は今のところ予定していません。
> 現時点ではマルチシーンによるコンテクストの親子関係を利用した設計を推奨しています。


## コンテクストの生成時に引数を渡す

コンテクストを生成する際に、コンテクストに対して引数を渡すことができます。
引数は、コンテクストのインストーラーに渡され、インストールする機能を切り替えたりする場合に有用です。

Unity のシーンは、引数を与えて挙動を切り替えるといったことができないため、工夫が必要ですが、
コンテクスト引数を使うことによって、シーンを切り替える際に、シーンに対して引数を渡すといった使い方ができます。

### 引数の渡し方

```SceneContextLoader``` もしくは、```GameObjectContextLoader``` の ```LoadAsync()```　メソッドの第二引数に、
```IContextArg``` を継承した型を渡すことで、コンテクストに対して引数を渡すことができます。

```csharp
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

### 引数の受け取り方

ロードされるコンテクストに配置されたインストーラーの ```Install()``` メソッドの第二引数に、```IContextArg``` が渡されます。

```csharp

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