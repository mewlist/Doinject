# コンテクスト空間の構造を確認する (DI Context Tree Window)

ランタイムでコンテクスト空間がどのような構造を持っているか、また、各コンテクストに属する DI コンテナの状態を確認できます。

Unity のメニューから、```Window > Doinject > DI Context Tree``` を選択してください。
以下のような Window が表示され、コンテクストの種類と親子関係を確認できます。

![DI Context Tree](DIContextTree_Bindings.png)

それぞれのコンテクストを選択すると、そのコンテクストに属する DI コンテナの状態を確認できます。

Bindings タブでは、DI コンテナに登録されている型とインスタンスの提供方法を確認できます。

Instances タブでは、DI コンテナによって生成されたインスタンスを確認できます。AsTransient や、Factory で生成されたインスタンスは、
DIコンテナの管理から外れるためここでは表示されません。

