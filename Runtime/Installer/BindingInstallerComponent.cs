using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public class BindingInstallerComponent : MonoBehaviour, IBindingInstaller
    {
        [field: SerializeField]
        protected List<BindingInstallerScriptableObject> InstallerScriptableObjects { get; set; }

        [field: SerializeField]
        protected List<BindingInstallerComponent> InstallerPrefabs { get; set; }

        [field: SerializeField]
        protected List<MonoBehaviour> ComponentBindings { get; set; }

        private ConcurrentBag<GameObject> InstalledPrefabs { get; } = new();

        private void OnValidate()
        {
#if UNITY_EDITOR
            InstallerPrefabs.RemoveAll(x =>
            {
                if (x is null) return false;
                var isPrefab = PrefabUtility.GetPrefabAssetType(x.gameObject) != PrefabAssetType.NotAPrefab;
                var scene = x.gameObject.scene;
                var isSceneValid = scene.IsValid();
                return !isPrefab || isSceneValid;
            });
#endif
        }

        public virtual void Install(DIContainer container, IContextArg contextArg)
        {
            foreach (var bindingScriptableObjectInstaller in InstallerScriptableObjects)
                bindingScriptableObjectInstaller.Install(container, contextArg);

            foreach (var installerPrefab in InstallerPrefabs)
            {
                var installer = Instantiate(installerPrefab);
                SceneManager.MoveGameObjectToScene(installer.gameObject, gameObject.scene);
                installer.Install(container, contextArg);
                container.PushInstance(installer);
                InstalledPrefabs.Add(installer.gameObject);
            }

            if (!ComponentBindings.Any()) return;

            var type = container.GetType();
            var methods = type.GetMethods();
            var method = methods.FirstOrDefault(x
                => x.Name == "BindFromInstance" &&
                   x.IsGenericMethodDefinition &&
                   x.GetGenericArguments().Length == 1);

            foreach (var component in ComponentBindings)
            {
                var componentType = component.GetType();
                var genericMethod = method.MakeGenericMethod(componentType);
                genericMethod.Invoke(container, new object[] { component });
            }
        }
    }
}