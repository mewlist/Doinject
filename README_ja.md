# Doinject
Asynchronous DI Container for Unity

![Logo.svg](Documentation%7E/images/Logo.svg)

![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
[![](https://img.shields.io/badge/license-MIT-blue)](https://github.com/mewlist/MewAssets/blob/main/LICENSE)

## ドキュメント

* [日本語ドキュメント](https://mewlist.github.io/Doinject)
* English document is not available yet.

## インストール

### Unity Package Manager でインストール

以下の順にパッケージをインストールしてください。

```
git@github.com:mewlist/MewCore.git
```

```
git@github.com:mewlist/Doinject.git
```

# Doinject について

Doinject は、Unity 向けの非同期 DI(Dependency Injection) フレームワークです。

非同期DIコンテナというコンセプトが起点となっています。
残念なお知らせですが、Unity 2022 以前のバージョンはサポートしません。

## コンセプト

### 非同期DIコンテナ

非同期なインスタンスの生成と解放をフレームワーク側でサポートします。
これにより、Addressables Asset Systems を通したインスタンスも扱うことができます。
また、カスタムファクトリを自分で作れば、時間のかかるインスタンスの生成を任せてしまうこともできます。

### Unity のライフサイクルと矛盾しないコンテクスト空間

Unity のライフサイクルと矛盾しない形でコンテクスト空間を定義するように設計されています。
シーンを閉じれば、シーンに紐づいたコンテクストを閉じ、そのコンテクスト空間で作成されたインスタンスが消え、
コンテクストを持つゲームオブジェクトを Destroy すれば、同様にコンテクストを閉じます。
コンテクスト空間はフレームワークによって自動的に構成され、複数のコンテクストがロードされた場合には、親子関係を作ります。

### Addressable Asset System との連携

Addressable Asset System のインスタンスも扱うことができ、ロードハンドルの解放を自動化することができます。
Addressables のリソース管理は、独自のリソースマネジメントシステムを作ったりと、慎重な実装が必要となりますが、
Doinject を使うと、Addressables のロード・解放を勝手にやってくれます。

### 平易な記述

ファクトリパターン、(コンテクストに閉じた)シングルトンパターン、サービスロケーターパターンの置き換えを、平易な記述で実現することができます。
また、カスタムファクトリやカスタムリゾルバを作ることで、より複雑なインスタンス生成シナリオに対応することができます。

## バインド記述の例

| 記述                                                                                  | インスタンス (仮想コード)                                                                                                                                                                        | インスタンスタイプ |
|-------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------|
| ```container.Bind<SomeClass>();```                                                  | ```new SomeClass()```                                                                                                                                                                 | cached    |
| ```container.Bind<SomeClass>().AsSingleton();```　                                   | ```new SomeClass()```                                                                                                                                                                 | singleton |
| ```container.Bind<SomeClass>().AsTransient();```　                                   | ```new SomeClass()```                                                                                                                                                                 | transient |
| ```container.Bind<SomeClass>().Args(123,"ABC");```　                                 | ```new SomeClass(123, "abc")```                                                                                                                                                       | cached    |
| ```container.Bind<ISomeInterface>().To<SomeClass>();```　                            | ```new SomeClass() as ISomeInterface```                                                                                                                                               | cached    |
| ```container.Bind<ISomeInterface, SomeClass>();```　                                 | ```new SomeClass() as ISomeInterface```                                                                                                                                               | cached    |
| ```container.Bind<SomeComponent>();```                                              | ```new GameObject().AddComponent<SomeComponent>()```                                                                                                                                  | cached    |
| ```container.Bind<SomeClass>().FromInstance(instance);```                           | ```instance```                                                                                                                                                                        | instance  |
| ```container.BindInstance(instance);```                                             | ```instance```                                                                                                                                                                        | instance  |
| ```container.BindPrefab<SomeComponent>(somePrefab);```                              | ```Instantiate(somePrefab).GetComponent<SomeComponent>()```                                                                                                                           | cached    |
| ```container.BindAssetReference<SomeAddressalbesObject>(assetReference);```    | ```var handle = Addressables.LoadAssetAsync<GameObject>(prefabAssetReference)```<br/>```var prefab = await handle.Task```                                                             | instance  |
| ```container.BindPrefabAssetReference<SomeComponent>(prefabAssetReference);``` | ```var handle = Addressables.LoadAssetAsync<GameObject>(prefabAssetReference)```<br/>```var prefab = await handle.Task```<br/>```Instantiate(prefab).GetComponent<SomeComponent>()``` | cached    |
| ```container.Bind<SomeClass>().AsFactory();```                                      | ```new Factory<SomeClass>(new TypeResolver<SomeClass>()) as IFactory<SomeClass>```                                                                                                    | factory   |
| ```container.Bind<SomeComponent>().AsFactory();```                                  | ```new Factory<SomeComponent>(new MonoBehaviourResolver())) as IFactory<SomeComponent>```                                                                                             | factory   |




