using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(ContextEntryPoint))]
    public class ContextEntryPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var entryPoint = (ContextEntryPoint) target;
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Reboot"))
                    entryPoint.Reboot();
            }
        }

        [MenuItem("GameObject/Doinject/Create Context Entry Point", false, 10)]
        public static void CreateContextEntryPoint()
        {
            if (FindFirstObjectByType<ContextEntryPoint>(FindObjectsInactive.Include))
            {
                Debug.LogWarning("There is already a Context Entry Point in the scene");
                return;
            }

            var go = new GameObject();
            var entryPoint = go.AddComponent<ContextEntryPoint>();
            entryPoint.name = "ContextEntryPoint";
            go.transform.SetSiblingIndex(0);
            Selection.activeObject = entryPoint;
            Undo.RegisterCreatedObjectUndo(go,"Create Context Entry Point");
        }
    }
}