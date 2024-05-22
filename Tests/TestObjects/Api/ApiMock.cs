using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class ApiMock
    {
        public async Task<Player> Get(PlayerId id)
        {
            await Task.Delay(300);
            return new Player
            {
                PlayerId = id,
                Name = $"Player_{id.Id}"
            };
        }
    }
}