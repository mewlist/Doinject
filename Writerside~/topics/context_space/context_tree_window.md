# コンテクスト空間の構造を確認する (DI Context Tree Window)

ランタイムでコンテクスト空間がどのような構造を持っているか、また、各コンテクストに属する DI コンテナの状態を確認できます。

Unity のメニューから、```Window > Doinject > DI Context Tree``` を選択してください。
以下のような Window が表示され、コンテクストの種類と親子関係を確認できます。

![DI Context Tree](DIContextTree_Bindings.png)

それぞれのコンテクストを選択すると、そのコンテクストに属する DI コンテナの状態を確認できます。

| カラム           | 説明         |
|---------------|------------|
| Type          | Bind されている型 |
| Resolver      | インスタンスの種類  |
| Strategy      | インスタンスの生成方法 |
| InstanceCount | インスタンスの数   |

AsTransient や、Factory で生成されたインスタンスは、 DIコンテナの管理から外れるためここでは表示されません。

