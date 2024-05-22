using System.Reflection;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
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
            var tasks = new ValueTask[methods.InjectMethods.Count];

            for (var i = 0; i < methods.InjectMethods.Count; i++)
                tasks[i] = ResolveAndInject(target, methods.InjectMethods[i], args, scopedInstances);

            await TaskHelper.WhenAll(tasks);
        }

        private async ValueTask ResolveAndInject<T>(T target, MethodInfo methodInfo, object[] args, ScopedInstance[] scopedInstances)
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