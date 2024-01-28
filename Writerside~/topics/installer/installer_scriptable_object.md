# ScriptableObject としてインストーラーを作成する

ScriptableObject としてインストーラーを作成する場合は、以下の手順に従ってください。

## インストーラスクリプトの作成

ScriptableObject としてインストーラーを作る場合は、`Doinject` メニューから作成することができます。Project ビューで右クリックし、
`Create` > `Doinject` > `Binding Installer ScriptableObject C# Script` を選択することで、 インストーラスクリプトが作成されます。

![CreateScriptableObjectInstaller](CreateBindingInsallerScriptableObjectScript.png)

CustomBindingInstallerScriptableObjectScript という名前でスクリプトを作成した場合、以下のようなスクリプトが作成されます。

```C#
using Doinject;
using UnityEngine;

[CreateAssetMenu(menuName = "Doinject/Installers/CustomBindingInstallerScriptableObjectScript", fileName = "CustomBindingInstallerScriptableObjectScript", order = 0)]
public class CustomBindingInstallerScriptableObjectScript : BindingInstallerScriptableObject
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
    }
}
```
```Install()``` 内に、バインディングを記述していきます。

## ScriptableObject アセットの作成

ここで作成された ScriptableObject は、Project ビュー内で右クリックをし、
`Create` > ```Doinject``` > ```Installers``` に表示され、選択することで ScriptableObject のアセットを作成することができます。

## ScriptableObjectInstaller をインストールする

インストーラーコンポーネントのインスペクタより、以下のように インストーラーの ScriptableObject を設定できます。

![InstallScriptableObjectInstaller](InstallScriptableObjectInstaller.png)
