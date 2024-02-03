using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    internal class ContextInternal : Context
    {
        public ContextInternal() : base()
        {
            ContextTracker.Instance.Add(this);
        }

        public ContextInternal(Scene scene, Context parentContext)
            : base(scene, parentContext)
        {
            ContextTracker.Instance.Add(this);
        }

        public ContextInternal(GameObject gameObject, Context parentContextContext)
            : base(gameObject, parentContextContext)
        {
            ContextTracker.Instance.Add(this);
        }

        public DIContainer RawContainer => container;

        public override ValueTask DisposeAsync()
        {
            ContextTracker.Instance.Remove(this);
            return base.DisposeAsync();
        }
    }
}