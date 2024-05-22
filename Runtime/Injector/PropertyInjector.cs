using System.Reflection;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;

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

            var tasks = new ValueTask[properties.InjectProperties.Count];
            for (var i = 0; i < properties.InjectProperties.Count; i++)
                tasks[i] = ResolveAndInject(target, properties.InjectProperties[i]);

            await TaskHelper.WhenAll(tasks);
        }

        private async ValueTask ResolveAndInject<T>(T target, PropertyInfo propertyInfo)
        {
            var instance = await Container.ResolveAsync(propertyInfo.PropertyType);
            propertyInfo.SetValue(target, instance);
        }
    }
}