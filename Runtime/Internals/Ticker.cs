using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mew.Core;
using UnityEngine;

namespace Doinject
{
    public class TickableMethod
    {
        public Action Invoke { get; }

        public TickableMethod(object target, MethodBase methodInfo)
        {
            if (target is MonoBehaviour monoBehaviour)
                Invoke = () =>
                {
                    if (monoBehaviour)
                        methodInfo.Invoke(monoBehaviour, null);
                };
            else
                Invoke = () => methodInfo.Invoke(target, null);
        }

    }

    internal class Ticker : IDisposable
    {
        private readonly Dictionary<TickableTiming, List<TickableMethod>> invokers = new();

        public Ticker()
        {
            MewLoop.Add<MewUnityEarlyUpdate>(InvokeEarlyUpdate);
            MewLoop.Add<MewUnityFixedUpdate>(InvokeFixedUpdate);
            MewLoop.Add<MewUnityPreUpdate>(InvokePreUpdate);
            MewLoop.Add<MewUnityUpdate>(InvokeUpdate);
            MewLoop.Add<MewUnityPreLateUpdate>(InvokePreLateUpdate);
            MewLoop.Add<MewUnityPostLateUpdate>(InvokePostLateUpdate);
        }

        public void Dispose()
        {
            MewLoop.Remove<MewUnityEarlyUpdate>(InvokeEarlyUpdate);
            MewLoop.Remove<MewUnityFixedUpdate>(InvokeFixedUpdate);
            MewLoop.Remove<MewUnityPreUpdate>(InvokePreUpdate);
            MewLoop.Remove<MewUnityUpdate>(InvokeUpdate);
            MewLoop.Remove<MewUnityPreLateUpdate>(InvokePreLateUpdate);
            MewLoop.Remove<MewUnityPostLateUpdate>(InvokePostLateUpdate);
            invokers.Clear();
        }

        private void InvokeEarlyUpdate() => Invoke(TickableTiming.EarlyUpdate);
        private void InvokeFixedUpdate() => Invoke(TickableTiming.FixedUpdate);
        private void InvokePreUpdate() => Invoke(TickableTiming.PreUpdate);
        private void InvokeUpdate() => Invoke(TickableTiming.Update);
        private void InvokePreLateUpdate() => Invoke(TickableTiming.PreLateUpdate);
        private void InvokePostLateUpdate() => Invoke(TickableTiming.PostLateUpdate);

        public void Add(object target, Dictionary<TickableTiming, List<MethodInfo>> methodsTickableMethods)
        {
            foreach (var (timing, methods) in methodsTickableMethods)
            foreach (var methodInfo in methods)
            {
                if (!invokers.ContainsKey(timing))
                    invokers[timing] = new List<TickableMethod>();
                invokers[timing].Add(new TickableMethod(target, methodInfo));
            }
        }

        private void Invoke(TickableTiming timing)
        {
            if (!invokers.TryGetValue(timing, out var methods)) return;

            foreach (var method in methods)
                method.Invoke();
        }
    }
}