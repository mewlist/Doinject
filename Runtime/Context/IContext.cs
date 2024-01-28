using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public interface IContext : IDisposable
    {
        Scene Scene { get; }
        GameObject ContextObject { get; }
        Context Context { get; }
        SceneContextLoader OwnerSceneContextLoader { get; }
        SceneContextLoader SceneContextLoader { get; }
        GameObjectContextLoader GameObjectContextLoader { get; }
        void SetArgs(IContextArg arg);
    }
}