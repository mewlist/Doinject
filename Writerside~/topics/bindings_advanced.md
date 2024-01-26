# 高度なバインディング

## カスタムファクトリ

ファクトリによる、インスタンスの生成手順が複雑な場合、バインディング記述だけでは対応できないケースも想定されます。
例えば、API 通信を挟むようなファクトリがあった場合、通信を行いその結果を利用してインスタンスを生成する必要があるかもしれません。

このような場合、カスタムファクトリを利用することで、ファクトリの生成手順をコードで記述できます。

すでに ISomeApi が DIコンテナにバインドされている前提とします。

```C#

internal interface ISomeApi
{
    public ValueTask<Player> GetPlayerAsync();
}
```

API 通信の結果を用いて ```IPlayer``` を生成するファクトリは以下のように ```IFactory<T>``` を継承して書くことができます。

```C#
private class AsyncPlayerFactory : IFactory<IPlayer>
{
    private ISomeApi Api { get; set; }

    [Inject] public void Construct(ISomeApi someApi)
    {
        Api = someApi;
    }

    public async ValueTask<IPlayer> CreateAsync()
    {
        var player = await Api.GetPlayerAsync();
        return player;
    }
}
```

```[Inject] Construct(...)``` で必要なインスタンスを注入してもらい、```CreateAsync()``` でインスタンスを生成する処理を記述します。

カスタムファクトリをバインドするには、以下のように ```AsCustomFactory<T＞``` に対して、カスタムファクトリの型を指定します。

```C#
container.Bind<IPlayer>()
    .AsCustomFactory<AsyncPlayerFactory>();
```

これで ```AsyncPlayerFactory``` が DIコンテナにバインドされます。

## カスタムリゾルバ

DIコンテナがインスタンスを生成する方法を、カスタマイズすることもできます。
例えば、レポジトリから ```IPlayer``` を取得するリゾルバは以下のように ```IResolver<T>``` を継承して書くことができます。

```C#
public class CustomResolver : IResolver<IPlayer>
{
    private PlayerId Id { get; set; }
    private IPlayerRepository PlayerRepository { get; set; }

    [Inject] public void Construct(PlayerId id, IPlayerRepository playerRepository)
    {
        Id = id;
        PlayerRepository = playerRepository;
    }

    public async ValueTask<IPlayer> ResolveAsync(IReadOnlyDIContainer container, object[] args)
    {
        return await PlayerRepository.Get(Id);
    }
}
```

カスタムファクトリ同様、必要なインスタンスを注入し、望む手段でインスタンスを生成できます。
```ResolveAsync``` の ```args``` 引数には、バインディング記述内の ```Args(...)``` に指定した引数が配列として渡されます。

カスタムリゾルバを使ってバインドするには、以下のように ```FromResolver<T＞``` を指定します。。

```C#
container.Bind<IPlayer>()
    .FromResolver<CustomResolver>();
```

## ファクトリを通じた引数渡し

コンストラクタやインジェクションポイントとなるメソッドが引数を要求する場合は、
引数の型を指定して ```AsFactory<TArg1, ...>()``` を呼び出します。

以下のような、コンストラクタに引数を持つ型を考えます。

```C#
private class SomeObject
{
    public SomeObject(int someArg)
    {
        ...
    }
}
```

このようにコンストラクタに引数をもつクラスの場合は、以下のように、int 型を指定します。

```C#
container.Bind<SomeObject>().AsFactory<int>()
```

引数を渡すには、```CreateAsync()``` の引数に渡します。

```C#
[Inject] public void Construct(IFactory<SomeObject> factory)
{
    var someObject = await factory.CreateAsync(123);
}
```

> AsFactory への引数指定は最大四つまで対応しており以下のインターフェースが定義されています。
>
> ```C#
> IFactory<TArg1, T>
> IFactory<TArg1, TArg2, T>
> IFactory<TArg1, TArg2, TArg3, T>
> IFactory<TArg1, TArg2, TArg3, TArg4, T>
> ```
{style="note"}

### カスタムファクトリを通じた引数渡し

引数の数に応じて、IFactory<TArg1, ... T> を継承してカスタムファクトリを定義してください。

```C#
private class SomeFactoryWithArgs : IFactory<int, IPlayer>
{
    public ValueTask<IPlayer> CreateAsync(int playerLevel)
    {
        ...
    }
}
```