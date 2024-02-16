using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            IEnumerable<ParameterInfo> parameterInfos,
            object[] args,
            ScopedInstance[] scopedInstances)
        {
            var parameters = new List<object>();
            var argsQueue = new Queue<object>(args ?? Array.Empty<object>());
            foreach (var parameterInfo in parameterInfos)
            {
                try
                {
                    var parameter = await Container.ResolveAsync(parameterInfo.ParameterType);
                    parameters.Add(parameter);
                    continue;
                }
                catch (FailedToResolveException)
                {
                    // ignored
                }

                if (argsQueue.Any() && argsQueue.Peek().GetType() == parameterInfo.ParameterType)
                {
                    parameters.Add(argsQueue.Dequeue());
                    continue;
                }

                var matched = scopedInstances
                    .FirstOrDefault(x => x.TargetType == parameterInfo.ParameterType);

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

            return parameters.ToArray();
        }
    }
}