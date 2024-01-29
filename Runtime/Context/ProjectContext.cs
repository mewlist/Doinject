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

        private Component EntryPointComponent { get; set; }

        public Scene Scene => default;
        public GameObject ContextObject => null;
        public Context Context { get; private set; }
        public SceneContextLoader OwnerSceneContextLoader => null;
        public SceneContextLoader SceneContextLoader { get; private set; }
        public GameObjectContextLoader GameObjectContextLoader { get; private set; }


        private void Initialize()
        {
            Context = new Context();
            Context.Container.Bind<IContextArg>().FromInstance(new NullContextArg());
            var go = new GameObject("ProjectContext");
            DontDestroyOnLoad(go);
            SceneContextLoader = go.AddComponent<SceneContextLoader>();
            SceneContextLoader.SetContext(this);
            GameObjectContextLoader = go.AddComponent<GameObjectContextLoader>();
            GameObjectContextLoader.SetContext(this);
            Context.InstallScriptableObjects(InstallerScriptableObjects);
            InstallPrefabs(InstallerPrefabs);
            Context.Container.GenerateResolvers().Forget();
        }

        private void InstallPrefabs(List<BindingInstallerComponent> installerPrefabs)
        {
            foreach (var installerPrefab in installerPrefabs)
            {
                var installer = Instantiate(installerPrefab);
                DontDestroyOnLoad(installer.gameObject);
                installer.Install(Context.Container, new NullContextArg());
            }
        }

        public void SetArgs(IContextArg arg)
        { }

        public void Dispose()
        {
            Context.DisposeAsync().Forget();
            Context = null;
        }
    }
}