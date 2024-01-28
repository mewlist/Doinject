using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Doinject
{
    internal static class SceneExtensions
    {
        public static T FindFirstObjectByType<T>(this Scene scene) where T : Object
        {
            var rootGameObjects = scene.GetRootGameObjects();
            return rootGameObjects
                .SelectMany(x => x.GetComponentsInChildren<T>())
                .FirstOrDefault(component => component);
        }

        public static T[] FindComponentsByType<T>(this Scene scene) where T : Object
        {
            var rootGameObjects = scene.GetRootGameObjects();
            return rootGameObjects
                .SelectMany(x => x.GetComponentsInChildren<T>())
                .ToArray();
        }

        public static UnityEngine.Component[] FindComponentsByType(this Scene scene, Type type)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            return rootGameObjects
                .SelectMany(x => x.GetComponentsInChildren(type))
                .ToArray();
        }
    }
}