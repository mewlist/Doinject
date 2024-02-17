using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Doinject
{
    internal class MethodInjector
    {
        private DIContainer Container { get; }
        private ParameterBuilder ParameterBuilder { get; }

        public MethodInjector(DIContainer container)
        {
            Container = container;
            ParameterBuilder = container.ParameterBuilder;
        }

        public async ValueTask DoInject<T>(
            T target,
            TargetMethodsInfo methods,
            object[] args,
            ScopedInstance[] scopedInstances)
        {
            var targetType = target.GetType();
            var resolverType = targetType;

            if (targetType == typeof(IInjectableComponent))
                resolverType = target.GetType();

            Container.MarkInjected(resolverType);

            foreach (var methodInfo in methods.InjectMethods)
            {
                var parameters = await ParameterBuilder.ResolveParameters(target.GetType(), methodInfo.GetParameters(), args, scopedInstances);
                if (methodInfo.IsTask()) await (Task)methodInfo.Invoke(target, parameters);
                else if (methodInfo.IsValueTask()) await (ValueTask)methodInfo.Invoke(target, parameters);
#if USE_UNITASK
                else if (methodInfo.IsUniTask()) await (UniTask)methodInfo.Invoke(target, parameters);
#endif
                else methodInfo.Invoke(target, parameters);
            }
        }
    }
}