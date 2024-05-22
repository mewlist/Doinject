using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal interface ISomeApi
    {
        public ValueTask<Player> GetPlayerAsync();
    }
}