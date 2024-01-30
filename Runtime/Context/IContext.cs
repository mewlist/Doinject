using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject
{
    public interface IContext : IDisposable
    {
        Scene Scene { get; }
        Context Context { get; }
        SceneContextLoader SceneContextLoader { get; }
        GameObjectContextLoader GameObjectContextLoader { get; }
    }
}