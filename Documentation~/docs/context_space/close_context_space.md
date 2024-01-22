# コンテクスト空間を閉じる

コンテクスト空間を閉じるにはいくつか方法があります。
状況に応じて適した方法を選択してください。

## Dispose を呼び出す

コンテクストに対して ```Dispose()``` を呼び出すことで、コンテクストを閉じることができます。
自身の属するコンテクスト空間のオブジェクトは、```IContext``` として、DIコンテナに登録されています。

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

## シーンコンテクスト空間: SceneContextLoader.UnloadAsync() を呼び出す

シーンコンテクストの場合は、シーンをロードした際に、SceneContext を保持しておくことで、そのコンテクストを閉じることができます。

```csharp
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
```

> [!NOTE]
> これ以外の方法で直接シーンを閉じたときでも、シーンコンテクスト自体は自動的に閉じられますが、
> Addressables でロードしたシーンをアンロードする場合は、そのハンドルが解放されなくなってしまうので、必ず、シーンローダーを使うか、コンテクストの破棄をするようにしてください。

## ゲームオブジェクトコンテクスト空間: Destroy() を呼び出す

ゲームオブジェクトコンテクストがアタッチされたオブジェクトを破棄すると、そのコンテクストは閉じます。

```csharp
Destroy(gameObjectContext);
```

