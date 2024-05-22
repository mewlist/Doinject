using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class PlayerApiResolver : IResolver<Player>
    {
        private ApiMock ApiMock { get; set; }
        public PlayerId PlayerId { get; set; }

        [Inject] public void Construct(ApiMock apiMock, PlayerId id)
        {
            ApiMock = apiMock;
            PlayerId = id;
        }

        public async ValueTask<Player> ResolveAsync(IReadOnlyDIContainer container, object[] args)
        {
            return await ApiMock.Get(PlayerId);
        }
    }
}