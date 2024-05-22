using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class SomeApi : ISomeApi
    {
        public async ValueTask<Player> GetPlayerAsync()
        {
            await Task.Delay(100);
            return new Player { Level = 55 };
        }
    }
}