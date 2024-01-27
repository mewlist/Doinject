# プロジェクトコンテクスト空間

プロジェクト全体を包み込むコンテクストです。
プロジェクトにプロジェクトコンテクストが設定されていた場合、アプリケーションの起動直後から有効になります。
*Scriptable Object インストーラー* を使って、プロジェクト全体で必要なバインディングの登録ができます。


## Project Context を作成する

プロジェクトコンテクストを有効にするためには、
<ui-path>Tools > Doinject > Create Project Context</ui-path>
メニューを選択します。

![CreateProjectContext.png](CreateProjectContext.png)

プロジェクトコンテクストは ```ScriptableObject``` として、<path>Assets/Resources</path> パスに作成されます。

![ProjectContextInspector.png](ProjectContextInspector.png)

インスペクタより、*ScriptableObject インストーラー*を登録することで、プロジェクト全域にわたって必要なバインディングを
インストールすることができます。