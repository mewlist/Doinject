# Doinject をはじめよう

## Unity Package Manager でインストール

Unity Package Manager からインストールすることができます。

### MewCore のインストール

<procedure id="procedure-install-mewcore">
<p>まず、依存ライブラリとなる、MewCore パッケージをインストールしてください。</p>
<step>Unity のメニューから <ui-path>Window > Package Manager</ui-path> を選択します。</step>
<step><control>+</control> ボタンをクリックし、<control>Add package from git URL...</control> を選択します。</step>
<step>以下を入力し、<control>Add</control> をクリックします。
<code-block>
git@github.com:mewlist/MewCore.git
</code-block>
</step>
</procedure>

### Doinject のインストール

<procedure id="procedure-install-doinject">
<p>次に、Doinject パッケージをインストールします。</p>
<step>Unity のメニューから <ui-path>Window > Package Manager</ui-path> を選択します。</step>
<step><control>+</control> ボタンをクリックし、<control>Add package from git URL...</control> を選択します。</step>
<step>以下を入力し、<control>Add</control> をクリックします。
<code-block>
git@github.com:mewlist/Doinject.git
</code-block>
</step>
</procedure>



## エントリポイントを決める

最初にすべきことは、DIフレームワークを適用したいシーンを開き、ヒエラルキにエントリポイントとなるコンポーネントを配置することです。

エントリポイントコンポーネントは、`ContextEntryPoint` という名前で、`Doinject` メニューから作成することができます。

![ContextMenu](CreateContextEntryPoint.png)

配置されたコンテクストエントリポイント

![ContextEntryPoint](ContextEntryPoint.png)

これで、このシーンは DI フレームワークの実行環境として機能するようになりました。
試しに一度再生してみてください。

![SceneContext](SceneContextCreated.png)

シーンが再生されると、このように自動的に SceneContext が生成されます。
SceneContext　は、フレームワークにより自動的に生成されるものですので、シーンには配置しないようにしてください。

> Doinject は、「コンテクスト空間」内でのみ、その機能へアクセスできます。
> 「コンテクスト空間」は、シーンやゲームオブジェクトに対して定義されます。また、ゲームオブジェクトコンテクストはヒエラルキの階層に従った空間を持ちます。
> コンテクストにはそれぞれ DIコンテナや、シーンローダーがぶら下がっており、DI フレームワークの実行環境として機能します。
{style="note"}

## 初めてのインジェクション

DIコンテナにインスタンスを登録して、インジェクションを行ってみましょう。
インスタンスの登録(バインディングと言います)はインストーラというコンポーネントから行います。

### 注入ポイントについて

DIコンテナに登録された型を、特定のインスタンスへ注入するためには、以下二通りの方法があります

: * コンストラクタインジェクション
: * メソッドインジェクション

コンストラクタインジェクションは、DIコンテナを通じてインスタンスを生成する際に自動的に行われます。
メソッドインジェクションは、[Inject] 属性をつけたメソッドを呼び出すことで行われます。

> DIコンテナを通じて生成されるインスタンスは、そのコンストラクタに自動的に依存注入を試みるため、[Inject]　属性は不要です。
> 明示的に [Inject] 属性をつけても問題はありませんが、その属性定義に依存してしまうことを考慮に入れる必要があります。
{style="note"}


MonoBehaviour を継承したコンポーネントの場合、コンストラクタインジェクションは使えません。
コンポーネントの生成は、Unity が行うため、明示的にコンストラクタを呼び出せないためです。
そのため、必ずメソッドインジェクションを使う必要があります。

### コンストラクタインジェクション

```C#
class SomeClass
{
    // SomeClass の生成時に、自動的に SomeDependency が注入されます
    public SomeClass(SomeDependency dependency)
    {
        ...
    }
}
```

### メソッドインジェクション

```C#
class SomeClass
{
    // [Inject] 属性をつけたメソッドは、インスタンスの生成後、DIコンテナによって呼び出され　SomeDependency を引数に渡します
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## OnInjected() コールバックについて

```OnInjected()``` というメソッドは(引数なし・public) は、
インスタンスの注入が完了したタイミングで、自動的に呼び出されます。
```Task```, ```ValueTask``` の戻り値を持つ非同期関数であっても問題ありません。

特に、MonoBehaviour を継承したコンポーネントの場合、Awake や Start などのライフサイクルメソッドと、
インジェクションが行われるタイミングが前後する可能性があります。
```OnInjected()``` は、インジェクションが完了した次のフレームで呼び出されるようになっているため、
初期化順を安定化させる目的で使用できます。

```C#
class SomeClass
{
    public SomeClass(SomeDependency dependency)
    {
        ...
    }

    // インスタンスの注入が完了したタイミングで、自動的に呼び出されます
    public void OnInjected()
    {
        ...
    }
}
```

## インストーラスクリプトの作成

インストーラは、`Doinject` メニューから作成することができます。Project ビューで右クリックし、
`Create` > `Doinject` > `Component Binding Installer C# Script` を選択することで、 インストーラスクリプトが作成されます。

![SceneContext](CreateComponentBindingInstaller.png)

CustomComponentBindingInstallerScript.cs という名前でスクリプトを作成した場合、以下のようなスクリプトが作成されます。

```C#
using Doinject;

public class CustomComponentBindingInstallerScript : BindingInstallerComponent
{
    public override void Install(DIContainer container)
    {
        // Bind your dependencies here
    }
}
```

また、動作を確認するため、DIコンテナに登録するオブジェクトと注入先のスクリプトを以下のように作成します。

### プレイヤーオブジェクト
```C#
using System;
using UnityEngine;

[Serializable]
public class Player
{
    [SerializeField] public string name;
    public Player(string name)
    {
        this.name = name;
    }
}
```

### 注入先スクリプト
```C#
using Doinject;
using UnityEngine;

public class InjectTargetScript : MonoBehaviour, IInjectableComponent
{
    [SerializeField] private Player player;

    [Inject] public void Construct(Player player)
    {
        this.player = player;
    }
}
```

Player オブジェクトのインスタンスが`InjectTargetScript.Construct()` を通じて
インジェクトされることがゴールとなります。

> MonoBehaviour を継承し、コンポーネントとして定義される型は、IInjectableComponent を継承する必要があります。
> また、[Inject] 属性をつけた、注入先のメソッドは public である必要があります。
{style="note"}

では、インストーラーにバインディングを記述していきましょう。
バインディングは、Install メソッド内で行います。

```C#
    public override void Install(DIContainer container)
    {
        container
            .Bind<Player>()
            .Args("Novice Player")
            .AsSingleton();
    }
```


`.Bind<Player>()` は、Player クラスをコンテナを通じて提供してもらうための宣言です。
`.Args("Novice Player")` は、Player クラスのコンストラクタにわたす引数となります。
`.AsSingleton()` は、コンテナを通じて提供される Player オブジェクトがシングルトンであることを示します。

バインド記述は、此のように Fluent Interface を通じて行います。
記述の仕方により、ファクトリとして振る舞ったり、インターフェースを介してインスタンスを提供してもらうようにしたりと柔軟な振る舞いの定義ができます。

> 便宜上 Singleton という用語をつかっていますが、あくまでもコンテクスト空間内で唯一という概念を表しています。
> シングルトンパターンとは異なり、コンテクストの境界を超えてインスタンスが共有されることはありません。
{style="note"}

これで、コンテクスト内で唯一となる Player オブジェクトが、DIコンテナに登録されました。

最後に、インストーラーをシーンに配置しましょう。

![](InstallerPlacedOnScene.png)

コンポーネントインストーラーは、基本的にシーンのどこに置いてあっても機能します。
また、複数の異なるインストーラーがあっても構いません。
わかりやすさのため、エントリポイントの下に置くなどルールを決めておくのが良いでしょう。

## 動作確認

先程つくった、InjectTargetScript コンポーネントを、シーンに配置しましょう。

![](InjectTargetPlacedOnScene.png)

では、シーンを再生してみます。

![](InjectTargetInjected.png)

Player の Name に "Novice Player" という文字列が設定されていることが確認できます。

Player のコンストラクタに Args で指定した引数が渡り(コンストラクタインジェクション)、
さらに、InjectTargetScript の Construct メソッドに Player オブジェクトが渡ったことが確認できました(メソッドインジェクション)。

> このとき、DIコンテナの内部では以下のような処理が行われていると考えて良いでしょう。
> ```C#
> var arg = "Novice Player";
> var player = new Player(arg);
> var injectable = Object.FindComponents(typeof(IInjectableComponent)).First();
> (injectable as InjectTargetScript).Construct(player)
> ```
{style="note"}

### 補足

シーン全体に配置されたオブジェクトは、すべてシーンコンテクストに包まれます。
: * シーンコンテクストは、生成時に、自身のコンテクストに属するインストーラーを探し、それらの定義に従ってバインディングを行います。
: * すべてのインストーラーのバインディングが完了すると、DIContainer は自身が扱うべき型とインスタンス解決方法を知っている状態となります。
: * その後、シーンコンテクストは、シーン内のすべてのゲームオブジェクトを探索し、IInjectableComponent を実装したコンポーネントを探します。
: * IInjectableComponent を実装したコンポーネントが見つかったら、そのコンポーネントの [Inject] 属性が指定されたメソッドの引数と型が一致するインスタンスを渡すよう呼び出します。
: * シーンコンテクストは、シーンのライフサイクルに合わせて生成され、シーンが破棄されると同時に、コンテナが抱えるインスタンスとともに破棄されます。

