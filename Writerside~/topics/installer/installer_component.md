# コンポーネントとしてインストーラーを作成する

コンポーネントとしてインストーラーを作成する場合は、以下の手順に従ってください。
インストーラーコンポーネントは、シーンに直接配置するか、プレハブ化して利用できます。

## インストーラスクリプトの作成

インストーラは、`Doinject` メニューから作成することができます。Project ビューで右クリックし、
`Create` > `Doinject` > `Binding Installer Component C# Script` を選択することで、 インストーラスクリプトが作成されます。

![SceneContext](CreateComponentBindingInstaller.png)

CustomComponentBindingInstallerScript.cs という名前でスクリプトを作成した場合、以下のようなスクリプトが作成されます。

```C#
using Doinject;

public class CustomComponentBindingInstallerScript : BindingInstallerComponent
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        // Bind your dependencies here
    }
}
```

```Install()``` 内に、バインディングを記述していきます。

## 作成したインストーラーをインストールする

インストーラーコンポーネントは、以下のいずれかの方法でインストールできます。

* コンテクストエントリポイントが存在するシーンに直接配置
* シーンローダーを経由してロードされるシーンに直接配置
* ゲームオブジェクトコンテクスト以下に配置
* プレハブのルートに配置し、プロジェクトコンテクスト、もしくは、他のインストーラーコンポーネントに設定

