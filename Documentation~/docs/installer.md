# インストーラー

DIコンテナに解決してもらう型やその解決方法を指定するために、インストーラーを利用します。

## インストーラーコンポーネントを作る

### インストーラスクリプトの作成

インストーラは、`Doinject` メニューから作成することができます。Project ビューで右クリックし、
`Create` > `Doinject` > `Binding Installer Component C# Script` を選択することで、 インストーラスクリプトが作成されます。

![SceneContext](../images/CreateComponentBindingInstaller.png)

CustomComponentBindingInstallerScript.cs という名前でスクリプトを作成した場合、以下のようなスクリプトが作成されます。

```csharp
using Doinject;

public class CustomComponentBindingInstallerScript : BindingInstallerComponent
{
    public override void Install(DIContainer container)
    {
        base.Install(container);
        // Bind your dependencies here
    }
}
```

```Install``` に提供された ```container``` を使ってバインディングを構築します。

### インストーラーコンポーネントをインストールする

インストーラーコンポーネントは、コンテクストエントリポイントの存在するシーンや、シーンローダーを経由してロードされるシーンにあらかじめ配置しておくか、
ゲームオブジェクトコンテクストの子として配置することで、インストールされます。

## インストーラーScriptableObject を作る

インストーラScriptableObject は、`Doinject` メニューから作成することができます。Project ビューで右クリックし、
`Create` > `Doinject` > `Binding Installer ScriptableObject C# Script` を選択することで、 インストーラスクリプトが作成されます。

![CreateScriptableObjectInstaller](../images/CreateBindingInsallerScriptableObjectScript.png)

CustomBindingInstallerScriptableObjectScript.cs という名前でスクリプトを作成した場合、以下のようなスクリプトが作成されます。

```csharp
using Doinject;
using UnityEngine;

[CreateAssetMenu(menuName = "Doinject/Installers/CustomBindingInstallerScriptableObjectScript", fileName = "CustomBindingInstallerScriptableObjectScript", order = 0)]
public class CustomBindingInstallerScriptableObjectScript : BindingInstallerScriptableObject
{
    public override void Install(DIContainer container)
    {
    }
}
```

```Install``` に提供された ```container``` を使ってバインディングを構築します。

ここで作成された ScriptableObject は、Project ビュー内で右クリックをし、
`Create` > ```Doinject``` > ```Installers``` から選択することで、作成することができます。

### ScriptableObjectInstaller をインストールする

インストーラーコンポーネントには、以下のように インストーラーの ScriptableObject を設定することができます。

![InstallScriptableObjectInstaller](../images/InstallScriptableObjectInstaller.png)

カスタムインストーラーコンポーネントが不要な場合は、ヒエラルキビューで右クリックをして、```Doinject``` -> ```Create Installer``` を選択することで、
空のインストーラーコンポーネントを作成することができます。

![CreateEmptyInstaller](../images/CreateEmptyInstaller.png)

## Unity オブジェクトとの連携

インストーラーの実態は、```MonoBehaviour``` もしくは ```ScriptableObject``` ですので、[SerializeField] プロパティを通じて、
他のオブジェクトを参照することができます。

Unity のオブジェクトを参照することで、それらをバインドすることができ、
プレハブの指定や、他のスクリプタブルオブジェクトのバインドができるようになっています。

```AssetReference``` や ```PrefabAssetReference``` を参照すれば ```Addressables``` で管理されたアセットをバインドすることもできます。