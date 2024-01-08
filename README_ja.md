# Doinject

Asynchronous DI Framework for Unity

![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/license-MIT-blue)

## Doinject とは

Doinject は、Unity 向けの非同期 DI(Dependency Injection) フレームワークです。

## DIってなに？

DI とは、Dependency Injection の略で、依存性の注入という意味です。

例を示します。

```csharp
class SomeClass
{
    private readonly SomeDependency dependency;
    public SomeClass()
    {
        var context = ContextManager.Instance;
        var playerName = PlayerRepository.Instance.GetPlayer().Name;
        dependency = new SomeDependency(context, playerName);
    }
}
```

SomeClass はコンストラクタで SomeDependency を new しています。
これは、SomeClass が SomeDependency に依存している状態です。
SomeDependency について、その生成手順まで知らなければなりませんし、SomeDependency　を必要とする他のクラスがある場合、生成処理が同様に書かれることになります。

だが、それはつらい。

そこで、SomeDependency のインスタンス化は、余所の誰かに任せてしまいましょう。
new するのではなく、コンストラクタの引数で渡してもらうようにしてみます。

```csharp
class SomeClass
{
    private readonly SomeDependency dependency;
    public SomeClass(SomeDependency dependency)
    {
        this.dependency = dependency;
    }
}
```

すっきりしました。Dependency Injection(依存の注入)とは、
このように、依存するオブジェクトを自分で new するのではなく、外部から渡してもらうようにすることです。

結果的に、new を呼び出してインスタンスを獲得していたコードは、呼び出されることでインスタンスを得ることになりました。
これを、制御の反転(IoC:Inversion of Control)と言い、DIフレームワークの重要な特徴となります。

## 依存関係の逆転

DI は、依存関係逆転の原則(Dependency Inversion Principle)を実現するためにも利用されます。
先のコードは