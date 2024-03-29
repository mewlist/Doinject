# はじめに

## Doinject とは

![Logo.svg](Logo.svg)

Doinject は、Unity 向けの非同期 DI(Dependency Injection) フレームワークです。

非同期DIコンテナというコンセプトが起点となっています。
Unity 2022 LTS / 2023.2 以降をサポートします。

![](https://img.shields.io/badge/unity-2022.3%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/unity-2023.2%20or%20later-green?logo=unity)
![](https://img.shields.io/badge/license-MIT-blue)

## コンセプト

### 非同期DIコンテナ

非同期なインスタンスの生成と解放をフレームワーク側でサポートします。
これにより、Addressables Asset Systems を通したインスタンスも生成できます。
また、カスタムファクトリを自分で作れば、時間のかかるインスタンスの生成も DIコンテナに任せられます。

### Unity のライフサイクルと矛盾しないコンテクスト空間

Unity のライフサイクルと矛盾しない形でコンテクスト空間を定義するように設計されています。
シーンを閉じれば、シーンに紐づいたコンテクストを閉じ、そのコンテクスト空間で作成されたインスタンスが消え、
コンテクストを持つゲームオブジェクトを Destroy すれば、同様にコンテクストを閉じます。

コンテクスト空間はフレームワークによって自動的に構成され、複数のコンテクストがロードされた場合には、親子関係を作ります。

### Addressable Asset System との連携

Addressable Asset System のインスタンスも扱え、ロードハンドルの解放を自動化できます。
Addressables のリソース管理は、独自のリソースマネジメントシステムを作ったり、慎重な実装が必要となることが多いと思いますが、
Doinject を使うと、Addressables のロード・解放を勝手にやってくれます。

### 平易な記述

ファクトリパターン、(コンテクストに閉じた)シングルトンパターン、サービスロケーターパターンの置き換えを、平易な記述で実現できます。
また、カスタムファクトリやカスタムリゾルバを作ることで、より複雑なインスタンス生成シナリオに対応できます。
