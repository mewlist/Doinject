using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(SceneContext))]
    public class SceneContextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var sceneContext = (SceneContext) target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Reboot"))
                    sceneContext.Reboot();
            }
        }

        [MenuItem("GameObject/Doinject/Create Scene Context", false, 10)]
        public static void CreateSceneContext()
        {
            if (FindFirstObjectByType<SceneContext>(FindObjectsInactive.Include))
            {
                Debug.LogWarning("There is already a Scene Context in the scene");
                return;
            }

            var go = new GameObject();
            var sceneContext = go.AddComponent<SceneContext>();
            sceneContext.name = "SceneContext";
            go.transform.SetSiblingIndex(0);
            Selection.activeObject = sceneContext;
            Undo.RegisterCreatedObjectUndo(go,"Create Scene Context");
        }
    }
}