# シーンを切り替える

シーンを切り替えるには、ロードしたシーンコンテクストをアンロードしてから、新しいシーンをロードします。
子のコンテクストとしてロードしたシーンは ```SceneContextLoader.UnloadAllScenesAsync()``` を呼び出すことで、
すべてアンロードできます。

```C#
await sceneContextLoader.UnloadAllAsync();
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

特定のシーンをアンロードするには、```LoadAsync()``` の戻り値を保持しておき、
必要なタイミングで ```UnloadAsync()``` を呼び出します。

```C#
var sceneContext = await sceneContextLoader.LoadAsync(firstScene, active: true);
...
await sceneContextLoader.UnloadAsync(sceneContext);
await sceneContextLoader.LoadAsync(nextScene, active: true);
```

## 自身のシーンコンテクスト空間を閉じて、別のシーンコンテクストを開く

自分自身の属するシーンコンテクストを閉じるには、```IContext.OwnerSceneContextLoader``` を経由します。

> ```Project Context``` 自身や、 ```Project Context```
> が設定されていないプロジェクトのエントリポイントコンテクストには、```OwnerSceneContextLoader``` が提供されていないので注意してください。
{style="note"}

```C#

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

> 一応、こういうこともできるという紹介で、この書き方が推奨できるかは微妙です。
> ここで紹介した記述を組み込んだ、アプリケーションの機構に適したにシーン管理クラスを用意するのが良いでしょう。
> シーン管理クラスをコンテクストにバインドして、子のコンテクストからも、シーン管理機能を通してシーンを切り替えるようにすると見通しが良くなります。
