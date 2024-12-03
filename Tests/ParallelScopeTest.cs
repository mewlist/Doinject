using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Doinject.Tests
{
    public class ParallelScopeTest
    {
        [Test]
        public async Task MultipleAwaitingTest()
        {
            var parallelScope = new ParallelScope();
            var result = 0;

            await Task.WhenAll(Wait(100), Wait(200), Wait(300), Cancel(1000));

            Assert.AreEqual(3, result);

            async Task Wait(int ms)
            {
                parallelScope.Begin();
                await Task.Delay(ms);
                parallelScope.End();
                await parallelScope.Wait();
                result++;
            }

            async Task Cancel(int ms)
            {
                await Task.Delay(ms);
                parallelScope.Cancel();
            }
        }
    }
}