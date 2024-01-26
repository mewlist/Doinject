# バインディング

DIコンテナに型やインスタンスを登録することをバインディングと呼びます。

インスタンス提供方法

* シングルトン
* ファクトリ
* インスタンスの固定
* 一時的生成(Transient)

オブジェクトの種類

* ゲームオブジェクト
* プレハブ
* Addressable Asset

これらを組み合わせて、様々なバインディングを柔軟に平易な記述で行うことができます。


## バインドする型の指定

### もっともシンプルなバインディング

```C#
container.Bind<SomeClass>();
```

```SomeClass``` という型を DI コンテナに登録します。インスタンスは、注入対象が存在した場合に、コンテクスト空間内でただ一つだけ生成され、
キャッシュされます(```AsCached()```と同じです)。

### インターフェース

```C#
container.Bind<ISomeInterface>()
    .To<SomeClass>();
```

```SomeClass``` が ```ISomeInterface``` を実装している必要があります。
```ISomeInterface``` に対して ```SomeClass``` のインスタンスが注入されます。

以下のようにもも書けたりしたりします。

```C#
container.Bind<ISomeInterface, SomeClass>();
```

### MonoBehaviour を継承した型

```C#
container.Bind<SomeComponent>();
```

通常のバインディングと記述は同じです。

> 型のバインディングでは、```MonoBehaviour``` を継承した型をバインドできます。
> 既定では、コンテクストの属するシーンのヒエラルキのトップレベルの位置に、対象のコンポーネントがアタッチされた ```GameObject``` が生成されます。
{style="note"}

以下のように、生成位置を指定できます。

* コンテクストの属するシーンのトップレベルに生成

  ```C#
  container.Bind<SomeComponent>()
    .UnderSceneRoot();
  ```

* 指定した ```Transfrom``` の子オブジェクトとして生成
    
    ```C#
    container.Bind<SomeComponent>()
        .Under(someTransform);
    ```

* 指定した ```GameObject``` に ```AddComponent```

    ```C#
    container.Bind<SomeComponent>()
        .On(someGameObject);
    ```


### インスタンス

```C#
container
    .Bind<SomeClass>()
    .FromInstance(someClassInstance);
```

someClassInstance がそのまま注入されます。

以下のように書くこともできます。

```C#
container.BindInstance(someClassInstance);
```

### プレハブからインスタンスを生成

```C#
container.BindPrefab<SomeComponent>(somePrefab);
```

SomeComponent がプレハブのルートオブジェクトにアタッチされている必要があります。

> 既定では、コンテクストの属するシーンのヒエラルキのトップレベルの位置に、対象のプレハブがインスタンス化されます。
{style="note"}

以下のように、生成位置を指定できます。

* コンテクストの属するシーンのトップレベルに生成

  ```C#
  container.Bind<SomeComponent>()
      .UnderSceneRoot();
  ```

* 指定した ```Transfrom``` の子オブジェクトとして生成

    ```C#
    container.BindPrefab<SomeComponent>(somePrefab)
      .Under(someTransform);
    ```


### Addressable Asset からロード {id="load-from-addressable-asset"}

```C#
AssetReference assetReference = ...;
container.BindAssetReference<SomeAddressalbesObject>(assetReference);
```

AsserReference は Addressable Asset System で提供される型で、アセットの参照を表します。

> ScriptableObject などのバインドに使うことを想定しています。
{style="note"}

### Addressables Asset からプレハブをロードしてインスタンスを生成

```C#
PrefabAssetReference prefabAssetReference = ...;
container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);
```

PrefabAssetReference は Doinject が用意している型で、プレハブを参照するための型です。
必ず、PrefabAssetReference を使ってバインディングする必要があります。
また、SomeComponent がプレハブのルートオブジェクトにアタッチされている必要があります。

>　既定では、コンテクストの属するシーンのヒエラルキのトップレベルの位置に、対象のプレハブがインスタンス化されます。
{style="note"}

以下のように、生成位置を指定するます。

* コンテクストの属するシーンのトップレベルに生成

    ```C#
    container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
        .UnderSceneRoot();
    ```

* 指定した ```Transfrom``` の子オブジェクトとして生成

    ```C#
    container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
        .Under(someTransform);
    ```

## インスタンス生成方法の指定

### キャッシュ

```C#
container.Bind<SomeClass>()
    .AsCached();
```

```AsCached()``` を呼び出すことで、キャッシュすることを明示的に示します。 記述がなくても、既定の動作としてキャッシュされます。
以下のようにも書けます。

```C#
container.BindCached<SomeClass>();　// 上記の省略形
```

### シングルトン

```C#
container.Bind<SomeClass>()
    .AsSingleton();

container.BindSingleton<SomeClass>();　// 上記の省略形
```

コンテクスト内でただ一つのインスタンスを生成します。
```AsCached()``` と異なり、注入対象が存在しない場合であっても、インスタンスを生成します。

> プレハブやコンポーネントをバインディングした際に、
> インスタンスをシーンに配置しておきたい場合や、生成された時点で何らかの非同期な機能をもったインスタンスを生成するのに使用します。
> 注入対象が存在する場合は、```AsCached()``` と同様にコンテクスト内でただ一つのインスタンスが注入されるので、動作に大きな違いはありません。
{style="note"}

### 一時的(Transient)

```C#
container.Bind<SomeClass>()
    .AsTransient();
```

AsTransient() で生成されるインスタンスは、注入対象ごとに、別々のインスタンスが生成されます。
生成されたインスタンスは、DIコンテナの管理から外れ、DIコンテナが解放されたタイミングで、自動的に破棄されない可能性があるため、注意が必要です。


## 引数

SomeClass が以下のようなコンストラクタを持つとします。

```C#
public SomeClass(int someInt, string someString)
{
    ...
}
```

このような型は、インスタンスの生成時に、引数を渡す必要があり、
Args() で引数を定義する必要があります。

```C#
container.Bind<SomeClass>()
    .Args(123, "some argument string");
```

> 引数は、コンストラクタインジェクションだけでなく、メソッドインジェクションに対しても同様に振舞います。
{style="note"}

### バインディングされている別の型を注入する

SomeClass が以下のようなコンストラクタを持つとします。

```C#
public SomeClass(ISomeInterface someInterface)
{
    ...
}
```

そして、ISomeInterface がバインディングされているとします。

```C#
container.Bind<ISomeInterface>()
    .To<SomeOtherClass>();

container.Bind<SomeClass>();
```

このような場合、```SomeClass``` 生成時に、```ISomeInterface``` に対して ```SomeOtherClass``` のインスタンスが自動的に注入されます。

次に、以下のような更に複雑なコンストラクタを持つケースを考えます。

```C#
public SomeClass(ISomeInterface someInterface, int someInt, string someString)
{
    ...
}
```

DIコンテナが解決できない引数を ```Args()``` で指定することで、インスタンスの生成が可能となります。

```C#
container.Bind<ISomeInterface>()
    .To<SomeOtherClass>();

container.Bind<SomeClass>()
    .Args(123, "some argument string");
```

Args に指定した引数の順序に従って、DIコンテナが解決できない引数に対して注入が行われます。
そのため、上記バインディング記述のまま、以下のようなコンストラクタに対しても問題なく注入が可能です。

```C#
public SomeClass(int someInt, ISomeInterface someInterface, string someString)
{
    ...
}
```

## ファクトリ

ファクトリを生成する方法は、バインディング記述に対して ```AsFactory()``` を呼び出すだけです。 
バインディング記述に応じた、```IFactory<T>``` が自動的にバインドされます。

```C#
container.Bind<SomeClass>()
    .AsFactory();
```

このように記述することで、```IFactory<SomeClass>``` がバインドされます。

ファクトリを定義することで、指定したインスタンスを生成するファクトリが注入できるようになります。
ファクトリインスタンスに対して、```CreateAsync()``` メソッドを呼び出すことで、インスタンスを生成できます。

```C#

IFactory<SomeClass> factory;
...
var instance = await factory.CreateAsync();
```

> インスタンス生成を行わない記述(```FromInstance()``` ```BindPrefabAssetReference()```)ではファクトリ生成記述はできません。
{style="note"}

ファクトリを使用するには、以下のように ```IFactory<SomeClass>``` を注入してもらいます。

```C#
public class SomeClass
{
    IFactory<SomeClass> factory;

    [Inject] async Task Construct(IFactory<SomeClass> factory)
    {
        this.factory = factory;　// ファクトリを保持
    }
    
    public async Task Create()
    {
        var instance = await factory.CreateAsync();　// 任意のタイミングでインスタンスを生成
    }
}
```

> ファクトリを介して生成されたインスタンスは、```AsTransient()``` と同様に、注入対象ごとに、別々のインスタンスが生成されます。
> また、生成されたインスタンスは、DIコンテナの管理から外れ、DIコンテナが解放されたタイミングで、自動的に破棄されない可能性があるため、注意が必要です。
{style="note"}

これまで示してきたバインディング書式をそのまま使用できるので、
自由に組み合わせて、様々なバインディングを柔軟に平易な記述で行うことができます。

以下、例を示します。

### インターフェースを返すファクトリ

```C#
container.Bind<ISomeInterface>()
    .To<SomeClass>()
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<ISomeInterface> がバインディングされます。
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```


### コンポーネントファクトリ

```C#
container.Bind<SomeComponent>()
    .On(someGameObject)
    .AsFactory();

// IFacoty<SomeComponent> がバインディングされます。
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```


### プレハブインスタンスファクトリ

```C#
container.BindPrefab<SomeComponent>(somePrefab)
    .Under(someTransform)
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<SomeComponent> がバインディングされます。
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```

### Addressable Asset プレハブインスタンスファクトリ

```C#
container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference)
    .UnderSceneRoot()
    .Args(123, "some argument string")
    .AsFactory();

// IFacoty<SomeComponent> がバインディングされます。
[Inject] async Task Construct(IFacoty<SomeComponent> factory)
{ ... }
```


