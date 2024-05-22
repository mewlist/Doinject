using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class AsyncPlayerFactory : IFactory<IPlayer>
    {
        private ISomeApi Api { get; set; }

        [Inject] public void Construct(ISomeApi someApi)
        {
            Api = someApi;
        }

        public async ValueTask<IPlayer> CreateAsync()
        {
            var player = await Api.GetPlayerAsync();
            return player;
        }
    }
}