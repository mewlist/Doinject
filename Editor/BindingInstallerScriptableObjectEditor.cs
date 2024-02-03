using UnityEditor;

namespace Doinject
{
    [CustomEditor(typeof(BindingInstallerScriptableObject))]
    public class BindingInstallerScriptableObjectEditor : Editor
    {
        [MenuItem("Assets/Create/Doinject/Binding Installer Scriptable Object C# Script", false, 10)]
        private static void CreateSimpleScript()
        {
            var path = AssetDatabase.GUIDToAssetPath("a0d1ea398c6345a98880870aa774cc24");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "CustomBindingInstallerScriptableObjectScript.cs");
        }
    }
}