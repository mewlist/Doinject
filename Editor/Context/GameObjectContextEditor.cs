using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(GameObjectContext))]
    public class GameObjectContextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var gameObjectContext = (GameObjectContext) target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Dispose"))
                    gameObjectContext.Dispose();
                if (GUILayout.Button("Reboot"))
                    gameObjectContext.Reboot();
            }
        }
    }
}