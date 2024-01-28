# ゲームオブジェクトコンテクスト空間

あるゲームオブジェクトを起点として、ヒエラルキ上の子全体をコンテクスト空間に包み込みます。
ゲームオブジェクトのツリーをコンテクストとして扱う場合は、
```GameObjectContext``` コンポーネントをそのルートオブジェクトに配置します。

ゲームオブジェクトコンテクスト空間を生成するには、以下のような方法があります。

* 直接シーンに配置する
* プレハブを作成しコンテクスト空間内にインスタンス化する

## 直接シーンに配置する

コンテクスト空間となるシーンに以下のようにゲームオブジェクトを配置します。

![GameObjectContext](GameObjectContextTree.png)

この例では、GameObjectContext という名前のノードがコンテクスト空間のルートとなり、子オブジェクトはすべてコンテクスト空間に包まれ、
シーンコンテクストと同様に、インストーラーや必要なコンポーネントを自由に配置できます。

まずは、ルートに GameObjectContext コンポーネントをアタッチします。

![GameObjectComponent](GameObjectContextComponent.png)

以上で、ゲームオブジェクトコンテクストの設定は完了です。
動作を確認すると、コンテクストツリーの状態が以下のようになっていることが確認できます。

![GameObjectContext ContextTree](GameObjectContextContextTree.png)

シーンコンテクストの子として、ゲームオブジェクトコンテクストが生成されていることが確認できます。
また、ゲームオブジェクトコンテクスト内に配置したインストーラの定義に従ったバインディングが行われていることも確認できます。

## プレハブを作成しコンテクスト空間内にインスタンス化する

先ほど作ったゲームオブジェクトコンテクストをプレハブ化し、
コンテクストが初期化されたあとに、プレハブをインスタンス化します。
ゲームオブジェクトコンテクストとなるプレハブをインスタンス化するには、
```GameObjectContextLoader.LoadAsync()``` を使います。
```GameObjectContextLoader``` は、```SceneContextLoader``` と同様に、DIコンテナに自動的にバインドされています。

```C#

[SerializeField] GameObject gameObjectContextPrefab;

public class SomeComponent : MonoBehaviour, IInjectableComponent
IContext context;

[Inject]
public void Construct(IContext context)
{
    this.context = context;
}

// OnInjected method is implicitly called after all dependencies are injected if defined.
public async Task OnInjected()
{
    await context.GameObjectContextLoader.LoadAsync(gameObjectContextPrefab);
}
```

## ```AutoContextLoader``` を使ってシーンをロードする

コンテクスト空間内の任意の場所に、```AutoContextLoader``` コンポーネントを配置することで、
プレハブ化されたゲームオブジェクトコンテクストをロードできます。

インスペクタの <control>Game Object Context Prefabs</control> にロードしたいプレハブを指定することで、
```AutoContextLoader```の属するコンテクスト空間がロードされた後、自動的に子コンテクストがロードされます。

![AutoContextLoader.png](AutoContextLoader.png)


## ゲームオブジェクトコンテクストのライフサイクル

このゲームオブジェクトコンテクストをプレハブ化し、再生中のシーンコンテクストにドラッグ＆ドロップすることで、
ゲームオブジェクトコンテクストがリアルタイムに生成されることが DI Context Tree Window で確認できます。

また、ヒエラルキビュー上でコンテクストルートを削除すると、
リアルタイムにコンテクストが破棄されることが、同様に DI Context Tree Window で確認できます。

このように、ゲームオブジェクトコンテクストは、Unity のライフサイクルと矛盾せず、開発中のデバッグがしやすいように設計されています。

ダイアログとして機能するUIの挙動を確認したいときなど、小さなコンテクストを持つモジュールを開発するとき、ゲームオブジェクトコンテクストは有用です。
