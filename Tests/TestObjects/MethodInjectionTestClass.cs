using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class MethodInjectionTestClass
    {
        public InjectedObject InjectedObjectSync { get; set; }
        public InjectedObject InjectedObjectASync { get; set; }
        public InjectedObject InjectedObjectValueTask { get; set; }

        [Inject]
        public void Inject(InjectedObject injectedObject)
        {
            InjectedObjectSync = injectedObject;
        }

        [Inject]
        public async Task InjectAsync(InjectedObject injectedObject)
        {
            await Task.Delay(100);
            InjectedObjectASync = injectedObject;
        }

        [Inject]
        public async ValueTask InjectValueTask(InjectedObject injectedObject)
        {
            await Task.Delay(100);
            InjectedObjectValueTask = injectedObject;
        }
    }
}