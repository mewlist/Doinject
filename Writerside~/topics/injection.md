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

メソッドインジェクションは、[Inject] 属性をつけたメソッドを呼び出すことで、依存を注入する方法です。

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

## OnInjected() コールバック


```OnInjected()``` というメソッドは(引数なし・public) は、
インスタンスの注入が完了したタイミングで、自動的に呼び出されます。
```Task```, ```ValueTask``` の戻り値を持つ非同期関数であっても問題ありません。

特に、MonoBehaviour を継承したコンポーネントの場合、Awake や Start などのライフサイクルメソッドと、
インジェクションが行われるタイミングが前後する可能性があります。
```OnInjected()``` は、インジェクションが完了した次のフレームで呼び出されるようになっているため、
初期化順を安定化させる目的で使用できます。

```C#
public class SomeClass
{
    // [Inject] 属性をつけたメソッドは、インスタンスの生成後、DIコンテナによって呼び出され　SomeDependency を引数に渡します
    [Inject]
    public InjectMethod(SomeDependency dependency)
    {
        ...
    }
    
    // インスタンスの注入が完了したタイミングで、自動的に呼び出されます
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

## コンポーネントへの遅延インジェクション (InjectableComponent)

コンテクスト空間の生成後、通常の Instantiate を使うなど、Factory などを通さずにコンポーネントを生成したときは、
通常は、そのコンポーネントに対してインジェクションは行われません。

通常このようなケースでは、Factory を使ってインスタンスを生成することで、インジェクションを行うことができますが、
いちいち Factory を用意するのも面倒です。

そんなときは、あらかじめ　```InjectableComponent``` を GameObject にアタッチすることで、
インスタンスが生成されたタイミングで、 同オブジェクトにアタッチされている他のコンポーネントへのインジェクションを行うことができます。

