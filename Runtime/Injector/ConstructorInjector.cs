using System;
using System.Linq;
using System.Threading.Tasks;

namespace Doinject
{
    internal class ConstructorInjector
    {
        private ParameterBuilder ParameterBuilder { get; }

        public ConstructorInjector(DIContainer container)
        {
            ParameterBuilder = container.ParameterBuilder;
        }

        public async ValueTask<object> DoInject(
            Type targetType,
            object[] args,
            ScopedInstance[] scopedInstances)
        {
            if (targetType.IsInterface)
                throw new Exception($"Instance type [{targetType.Name}] is interface".ToExceptionMessage());

            var constructors = targetType.GetConstructors();
            var constructor = constructors.First();
            var parameters = constructor.GetParameters();
            object instance;
            if (parameters.Any())
            {
                var buildParameters = await ParameterBuilder.ResolveParameters(targetType, parameters, args ?? Array.Empty<object>(), scopedInstances);
                try
                {
                    instance = constructor.Invoke(buildParameters);
                }
                catch (Exception e)
                {
                    if (e.InnerException is InvalidOperationException)
                        throw e.InnerException;
                    throw;
                }
            }
            else
            {
                instance = Activator.CreateInstance(targetType);
            }

            return instance;
        }
    }
}