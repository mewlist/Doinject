# コンテクスト空間を閉じる

コンテクスト空間を閉じるにはいくつか方法があります。
状況に応じて適した方法を選択してください。

## Dispose を呼び出す

コンテクストに対して ```Dispose()``` を呼び出すことで、コンテクストを閉じることができます。
自身の属するコンテクスト空間のオブジェクトは、```IContext``` として、DIコンテナに登録されています。

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

## シーンコンテクスト空間: SceneContextLoader.UnloadAsync() を呼び出す

シーンコンテクストの場合は、シーンをロードした際に、SceneContext を保持しておくことで、そのコンテクストを閉じられます。

```C#
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
```

## SceneManager でアンロードする

通常の Unity が提供するシーンのアンロード機能を使って、シーンコンテクストを閉じても構いません。

```C#
SceneManager.UnloadSceneAsync(targetScene);
```

## ゲームオブジェクトコンテクスト空間: Destroy() を呼び出す

ゲームオブジェクトコンテクストがアタッチされたオブジェクトを破棄すると、そのコンテクストは閉じます。

```C#
Destroy(gameObjectContext);
```

