using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(GameObjectContextLoader))]
    public class GameObjectContextLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var goCtxLoader = (GameObjectContextLoader) target;

            GUILayout.Label("Loaded GameObjects:");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            foreach (var child in goCtxLoader.ReadOnlyChildContexts)
            {
                if (child.Initialized) GUILayout.Label(child.Context.ToString());
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Unload All"))
                    goCtxLoader.UnloadAllContextsAsync();
            }
        }
    }
}