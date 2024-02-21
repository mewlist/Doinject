using System.Threading.Tasks;

namespace Doinject
{
    internal class PropertyInjector
    {
        private DIContainer Container { get; }
        private ParameterBuilder ParameterBuilder { get; }

        public PropertyInjector(DIContainer container)
        {
            Container = container;
            ParameterBuilder = container.ParameterBuilder;
        }

        public async ValueTask DoInject<T>(
            T target,
            TargetPropertiesInfo properties)
        {
            if (!properties.Any()) return;
            var targetType = target.GetType();
            var resolverType = targetType;

            if (targetType == typeof(IInjectableComponent))
                resolverType = target.GetType();

            Container.MarkInjected(resolverType);

            foreach (var propertyInfo in properties.InjectProperties)
            {
                var instance = await Container.ResolveAsync(propertyInfo.PropertyType);
                propertyInfo.SetValue(target, instance);
            }
        }
    }
}