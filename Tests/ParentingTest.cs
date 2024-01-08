using System.Threading.Tasks;
using NUnit.Framework;

namespace Doinject.Tests
{
    public class ParentingTest
    {
        private DIContainer container;

        [SetUp]
        public void Setup()
        {
            container = new DIContainer();
        }

        [TearDown]
        public async Task TearDown()
        {
            await container.DisposeAsync();
        }

        [Test]
        public async Task ResolveInstanceInParent()
        {
            var injectedInstance = new InjectedObject();
            var parent = new DIContainer();
            var child = new DIContainer(parent);
            var grandChild = new DIContainer(child);

            parent.BindFromInstance(injectedInstance);
            grandChild.Bind<InjectableObject>();

            var instance = await grandChild.ResolveAsync<InjectableObject>();
            var instanceB = await grandChild.ResolveAsync<InjectedObject>();
            Assert.AreSame(injectedInstance, instance.InjectedObject);
            Assert.AreSame(injectedInstance, instanceB);

            await parent.DisposeAsync();
            await child.DisposeAsync();
            await grandChild.DisposeAsync();
        }
    }
}