using System;
using UnityEngine;

namespace Doinject.Context
{
    public interface IContext : IDisposable
    {
        GameObject ContextObject { get; }
        Context Context { get; }
        SceneContextLoader OwnerSceneContextLoader { get; }
    }
}