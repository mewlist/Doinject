using Mew.Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(SceneContextLoader))]
    public class SceneContextLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var sceneCoordinator = (SceneContextLoader) target;

            GUILayout.Label("Loaded Scenes:");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            foreach (var child in sceneCoordinator.ReadonlyChildSceneContexts)
            {
                GUILayout.Label(child.Context.ToString());
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Unload All Scenes"))
                    sceneCoordinator.UnloadAllScenesAsync().Forget();
            }
        }
    }
}