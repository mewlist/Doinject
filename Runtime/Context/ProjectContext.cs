using System.Collections.Generic;
using Mew.Core.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    [CreateAssetMenu(menuName = "Doinject/Create ProjectContext", fileName = "ProjectContext", order = 0)]
    public class ProjectContext : ScriptableObject, IContext
    {
        private static ProjectContext instance;
        public static ProjectContext Instance => instance ? instance : instance = Resources.Load<ProjectContext>("ProjectContext");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            if (Instance == null)
                return;

            Instance.Initialize();
        }

        [field: SerializeField]
        private List<BindingInstallerScriptableObject> InstallerScriptableObjects { get; set; }

        [field: SerializeField]
        protected List<BindingInstallerComponent> InstallerPrefabs { get; set; }

        private ContextInternal ContextInternal { get; set; }

        public Scene Scene => default;
        public Context Context => ContextInternal;
        public SceneContextLoader OwnerSceneContextLoader => null;
        public SceneContextLoader SceneContextLoader { get; private set; }
        public GameObjectContextLoader GameObjectContextLoader { get; private set; }
        public bool IsReverseLoaded => false;
        public bool InjectionProcessing => ContextInternal.RawContainer.InjectProcessing;


        private void Initialize()
        {
            ContextInternal = new ContextInternal();
            ContextInternal.RawContainer.Bind<IContextArg>().FromInstance(new NullContextArg());
            var go = new GameObject("ProjectContext");
            DontDestroyOnLoad(go);
            SceneContextLoader = go.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);
            GameObjectContextLoader = go.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);
            ContextInternal.RawContainer.Bind<IContext>().FromInstance(this);
            Context.Install(InstallerScriptableObjects, new NullContextArg());
            InstallPrefabs(InstallerPrefabs);
            ContextInternal.RawContainer.GenerateResolvers().Forget();
        }

        private void InstallPrefabs(List<BindingInstallerComponent> installerPrefabs)
        {
            foreach (var installerPrefab in installerPrefabs)
            {
                var installer = Instantiate(installerPrefab);
                DontDestroyOnLoad(installer.gameObject);
                installer.Install(ContextInternal.RawContainer, new NullContextArg());
            }
        }

        public void SetArgs(IContextArg arg)
        { }

        public void Dispose()
        {
            ContextInternal.DisposeAsync().Forget();
            ContextInternal = null;
        }
    }
}