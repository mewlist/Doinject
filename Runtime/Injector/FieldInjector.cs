using System.Reflection;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;

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

            var tasks = new ValueTask[fields.InjectFields.Count];

            for (var i = 0; i < fields.InjectFields.Count; i++)
                tasks[i] = ResolveAndInject(target, fields.InjectFields[i]);

            await TaskHelper.WhenAll(tasks);
        }

        private async ValueTask ResolveAndInject<T>(T target, FieldInfo fieldInfo)
        {
            var instance = await Container.ResolveAsync(fieldInfo.FieldType);
            fieldInfo.SetValue(target, instance);
        }
    }
}