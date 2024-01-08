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
                if (GUILayout.Button("Dispose"))
                    sceneContext.Dispose();
            }
        }
    }
}