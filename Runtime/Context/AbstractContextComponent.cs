using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mew.Core.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public abstract class AbstractContextComponent : MonoBehaviour, IContext
    {
        protected TaskQueue TaskQueue { get; } = new();
        protected IContextArg Arg { get; private set; } = new NullContextArg();


        public abstract Scene Scene { get; }
        public Context Context { get; protected set; }
        public SceneContextLoader SceneContextLoader { get; protected set; }
        public GameObjectContextLoader GameObjectContextLoader { get; protected set; }


        protected abstract IEnumerable<T> GetComponentsUnderContext<T>();


        protected virtual void Awake()
        {
            TaskQueue.Start(destroyCancellationToken);
        }

        public void SetArgs(IContextArg arg)
        {
            Arg = arg ?? new NullContextArg();
        }

        protected async Task InjectIntoUnderContextObjects()
        {
            var targets = GetComponentsUnderContext<IInjectableComponent>()
                .Where(x => x.enabled);
            await Task.WhenAll(targets.Select(x
                => Context.Container.InjectIntoAsync(x).AsTask()));
        }

        public void Dispose()
        {
            if (this) Destroy(this);
        }
    }
}