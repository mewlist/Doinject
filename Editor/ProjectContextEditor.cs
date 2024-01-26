using System.IO;
using UnityEditor;
using UnityEngine;

namespace Doinject.Context
{
    [CustomEditor(typeof(ProjectContext))]
    public class ProjectContextEditor : Editor
    {
        [MenuItem("Tools/Doinject/Create Project Context", false, 10)]
        public static void CreateInstaller()
        {
            if (ProjectContext.Instance != null)
                Debug.LogWarning("ProjectContext already exists.", ProjectContext.Instance);

            var so = CreateInstance<ProjectContext>();
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.CreateAsset(so, "Assets/Resources/ProjectContext.asset");
            AssetDatabase.SaveAssets();
        }
    }
}