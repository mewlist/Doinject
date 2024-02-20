using System.Collections.Generic;
using Mew.Core.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public abstract class AbstractContextComponent : MonoBehaviour, IContext
    {
        internal ContextInternal ContextInternal { get; set; }

        protected TaskQueue TaskQueue { get; } = new(TaskQueueLimitType.SwapLast, 2);
        protected IContextArg Arg { get; private set; } = new NullContextArg();


        public abstract Scene Scene { get; }
        public Context Context => ContextInternal;
        public SceneContextLoader SceneContextLoader { get; protected set; }
        public GameObjectContextLoader GameObjectContextLoader { get; protected set; }
        public abstract bool IsReverseLoaded { get; }
        public abstract bool Loaded { get; }
        public bool InjectionProcessing => Context.InjectProcessing;

        protected abstract IEnumerable<T> GetComponentsUnderContext<T>();


        protected virtual void Awake()
        {
            TaskQueue.DisposeWith(destroyCancellationToken);
        }

        public void SetArgs(IContextArg arg)
        {
            Arg = arg ?? new NullContextArg();
        }

        public void Dispose()
        {
            if (this) Destroy(this);
        }
    }
}