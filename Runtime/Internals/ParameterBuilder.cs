using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;

namespace Doinject
{
    internal class ParameterBuilder
    {
        private DIContainer Container { get; }

        public ParameterBuilder(DIContainer container)
        {
            Container = container;
        }

        public async ValueTask<object[]> ResolveParameters(
            Type targetType,
            IReadOnlyList<ParameterInfo> parameterInfos,
            object[] args,
            ScopedInstance[] scopedInstances)
        {
            var parameters = new List<object>();
            var argsQueue = new Queue<object>(args ?? Array.Empty<object>());

            var resolveTasks = new ValueTask<object>[parameterInfos.Count];
            for (var i = 0; i < parameterInfos.Count; i++)
                resolveTasks[i] = TryResolve(parameterInfos[i]);
            var resolved = await TaskHelper.WhenAll(resolveTasks);

            for (var i = 0; i < resolved.Length; i++)
            {
                if (resolved[i] is not null)
                {
                    parameters.Add(resolved[i]);
                    continue;
                }

                TryFillParameters(
                    targetType,
                    parameterInfos[i],
                    scopedInstances,
                    parameters,
                    argsQueue);
            }

            return parameters.ToArray();
        }

        private async ValueTask<object> TryResolve(
            ParameterInfo parameterInfo)
        {
            try
            {
                var parameter = await Container.ResolveAsync(parameterInfo.ParameterType);
                return parameter;
            }
            catch (FailedToResolveException)
            {
                return null;
            }
        }

        private void TryFillParameters(
            MemberInfo targetType,
            ParameterInfo parameterInfo,
            ScopedInstance[] scopedInstances,
            List<object> parameters,
            Queue<object> argsQueue)
        {
            if (argsQueue.Any() && argsQueue.Peek().GetType() == parameterInfo.ParameterType)
            {
                parameters.Add(argsQueue.Dequeue());
                return;
            }

            ScopedInstance matched = default;
            foreach (var x in scopedInstances)
            {
                if (x.TargetType != parameterInfo.ParameterType) continue;
                matched = x;
                break;
            }

            if (matched.IsValid)
            {
                parameters.Add(matched.Instance);
            }
            else
            {
                var optionalAttribute = parameterInfo.GetCustomAttribute(typeof(OptionalAttribute), true);
                var parameterType = parameterInfo.ParameterType;

                if (optionalAttribute is null)
                    throw new FailedToResolveParameterException(parameterInfo.ParameterType, targetType);

                parameters.Add(parameterType.IsValueType
                    ? Activator.CreateInstance(parameterType)
                    : null);
            }
        }
    }
}