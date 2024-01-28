# 空のインストーラーコンポーネントを配置する

ScriptableObject、もしくは、プレハブとして作成されたインストーラーのみを使用する場合、、
新しくインストーラーコンポーネントのクラスを作らずとも、空のインストーラーを配置するだけで十分でしょう。

ヒエラルキビューで右クリックをして、```Doinject``` -> ```Create Installer``` を選択することで、
空のインストーラーコンポーネントを作成できます。
ここで作成された空のインストーラーを使っても、ScriptableObject インストーラー・Prefab インストーラーを指定できます。

![CreateEmptyInstaller](CreateEmptyInstaller.png)

インスペクタより、ScriptableObject インストーラー・Prefab インストーラーを指定できます。

![EmptyInstallerComponentInspector.png](EmptyInstallerComponentInspector.png)

