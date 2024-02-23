# インジェクション

依存を注入する箇所について説明します。

## コンストラクタインジェクション

コンストラクタインジェクションは、コンストラクタの引数に依存を注入する方法です。

```C#
public class SomeClass
{
    // SomeClass の生成時に、自動的に SomeDependency が注入されます
    public SomeClass(SomeDependency dependency)
    {
        ...
    }
}
```

## メソッドインジェクション

メソッドインジェクションは、```[Inject]``` 属性をつけたメソッドを呼び出すことで、依存を注入する方法です。

```C#
public class SomeClass
{
    // [Inject] 属性をつけたメソッドは、インスタンスの生成後、DIコンテナによって呼び出され　SomeDependency を引数に渡します
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
}
```

## フィールドインジェクション

フィールドインジェクションは、```[Inject]``` 属性をつけた ```public``` なフィールドに対して、依存を注入する方法です。

```C#
public class SomeClass
{
    // [Inject] 属性をつけた public なフィールドに SomeDependency を注入します
    [Inject] public SomeDependency dependency;
    ...
}
```

## プロパティインジェクション

プロパティインジェクションは、```[Inject]``` 属性をつけた ```public``` な setter を持つプロパティに対して、依存を注入する方法です。

```C#
public class SomeClass
{
    // [Inject] 属性をつけた public な setter を持つプロパティに SomeDependency を注入します
    [Inject] public SomeDependency Dependency {get; set;}
    ...
}
```

## OnInjected コールバック

```OnInjected``` 属性を付けたメソッドは(引数なし・public) は、
インスタンスの注入が完了したタイミングで、自動的に呼び出されます。
```Task```, ```ValueTask```, ```UniTask``` の戻り値を持つ非同期関数であっても問題ありません。

特に、MonoBehaviour を継承したコンポーネントの場合、Awake や Start などのライフサイクルメソッドと、
インジェクションが行われるタイミングが前後する可能性があります。
```OnInjected``` コールバックは、インジェクションが完了した次のフレームで呼び出されるようになっているため、
コンポーネントの初期化を安定させる目的で使用します。

```C#
public class SomeClass
{
    // [Inject] 属性を付与したメソッドは、インスタンスの生成後、DIコンテナによって呼び出され　SomeDependency を引数に渡します
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
    
    // [OnInjected] 属性を付与したメソッドはインスタンスの注入が完了したタイミングで、自動的に呼び出されます
    [OnInjected]
    public void OnInjected()
    {
        ...
    }
}
```

## オプショナルインジェクション

```[Optional]```属性を引数に指定することで、依存が解決できない場合、インジェクションをスキップできます。
スキップされた引数には、その型の ```default``` 値が渡されます。

```C#
public class SomeClass
{
    // SomeDependency の依存を解決できない場合、null が渡されます
    public SomeClass([Optional] SomeDependency dependency)
    {
        ...
    }
}
```

> 特定のコンテクストを要求するようなオブジェクトを別のコンテクストでも動作できるようにします。
> インジェクションがスキップされた場合に既定値を設定し、オブジェクトの動作を保証し、デバッグ効率を上げられるでしょう。
> また、コンテクストによって、動作が切り替わるようなオブジェクトの実現にも役立ちます。
{style="note"}

## コンポーネントへの動的インジェクション (DynamicInjectable)

コンテクスト空間の生成後、通常の Instantiate を使うなど、Factory などを通さずにコンポーネントを生成したときは、
通常は、そのコンポーネントに対してインジェクションは行われません。

通常このようなケースでは、Factory を使ってインスタンスを生成することで、インジェクションを行うことができますが、
いちいち Factory を用意するのも面倒です。

そんなときは、あらかじめ　```DynamicInjectable``` コンポーネントを GameObject にアタッチすると、
```Object.Instantiate``` で、インスタンスが生成されたタイミングでも、 同オブジェクトにアタッチされている
IInjectableComponent を継承したコンポーネントへのインジェクションを行うことができます。

DI コンテナを使いながらも、Unity の通常のコーディングフローに従った開発をサポートします。
