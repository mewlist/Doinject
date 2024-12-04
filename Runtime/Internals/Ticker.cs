using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mew.Core;
using UnityEngine;

namespace Doinject
{
    internal class Ticker : IDisposable
    {
        private readonly Dictionary<TickableTiming, ConditionalWeakTable<object, MethodBase>> invokers = new();

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
                    invokers[timing] = new();

                invokers[timing].Add(target, methodInfo);
            }
        }

        private void Invoke(TickableTiming timing)
        {
            if (!invokers.TryGetValue(timing, out var methods)) return;

            List<object> removeTargets = null;
            foreach (var x in methods)
            {
                var target = x.Key;
                var method = x.Value;

                if (target is MonoBehaviour monoBehaviour)
                {
                    if (monoBehaviour)
                    {
                        method.Invoke(monoBehaviour, null);
                    }
                    else
                    {
                        removeTargets ??= new List<object>();
                        removeTargets.Add(target);
                    }
                }
                else
                    method.Invoke(target, null);
            }

            if (removeTargets != null)
            {
                foreach (var x in removeTargets)
                    methods.Remove(x);
                removeTargets.Clear();
            }
        }

        public bool Any(TickableTiming timing)
        {
            if (!invokers.TryGetValue(timing, out var methods)) return false;
            return methods.Any();
        }
    }
}