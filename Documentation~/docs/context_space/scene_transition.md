# シーンを切り替える

シーンを切り替えるには、ロードしたシーンコンテクストをアンロードしてから、新しいシーンをロードします。
子のコンテクストとしてロードしたシーンは ```SceneContextLoader.UnloadAllScenesAsync()``` を呼び出すことで、
すべてアンロードすることができます。

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

## 自身のシーンコンテクスト空間を閉じて、別のシーンコンテクストを開く

自分自身の属するシーンコンテクストを閉じるには、```IContext.OwnerSceneContextLoader``` を経由します。

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

> [!NOTE]
> エントリポイントコンテクストには、```OwnerSceneContextLoader``` が提供されていないので注意してください。

> [!TIP]
> 一応、こういうこともできるという紹介で、この書き方が推奨できるかは微妙です。
> ここで紹介した記述を組み込んだ、アプリケーションの機構に適したにシーン管理クラスを用意するのが良いでしょう。
> シーン管理クラスをコンテクストにバインドして、子のコンテクストからも、シーン管理機能を通してシーンを切り替えるようにすると見通しが良くなります。

> [!IMPORTANT]
> Doinject では、プロジェクト全体を表現するコンテクスト空間は今のところ予定していません。
> 現時点ではマルチシーンによるコンテクストの親子関係を利用した設計を推奨しています。

