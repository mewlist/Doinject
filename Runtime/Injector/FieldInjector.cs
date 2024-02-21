using System.Threading.Tasks;

namespace Doinject
{
    internal class FieldInjector
    {
        private DIContainer Container { get; }
        private ParameterBuilder ParameterBuilder { get; }

        public FieldInjector(DIContainer container)
        {
            Container = container;
            ParameterBuilder = container.ParameterBuilder;
        }

        public async ValueTask DoInject<T>(
            T target,
            TargetFieldsInfo fields)
        {
            if (!fields.Any()) return;
            var targetType = target.GetType();
            var resolverType = targetType;

            if (targetType == typeof(IInjectableComponent))
                resolverType = target.GetType();

            Container.MarkInjected(resolverType);

            foreach (var fieldInfo in fields.InjectFields)
            {
                var instance = await Container.ResolveAsync(fieldInfo.FieldType);
                fieldInfo.SetValue(target, instance);
            }
        }
    }
}