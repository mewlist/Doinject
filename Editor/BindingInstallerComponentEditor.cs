using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(BindingInstallerComponent))]
    public class BindingInstallerComponentEditor : Editor
    {
        [MenuItem("GameObject/Doinject/Create Installer", false, 10)]
        public static void CreateInstaller()
        {
            var go = new GameObject();
            var installer = go.AddComponent<BindingInstallerComponent>();
            var selection = Selection.activeGameObject;
            installer.name = "BindingInstaller";
            Selection.activeObject = installer;
            if (selection != null) go.transform.SetParent(selection.transform);
            Undo.RegisterCreatedObjectUndo(go,"Create Binding Installer");
        }

        [MenuItem("Assets/Create/Doinject/Binding Installer Component C# Script", false, 10)]
        private static void CreateBindingInstallerComponent()
        {
            var path = AssetDatabase.GUIDToAssetPath("a594afb0055a4ebfb446b23245395c18");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "CustomBindingInstallerComponentScript.cs");
        }
    }
}