using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class WithPlayerResolverFactory : Factory<IPlayer>
    {
        private PlayerLevel PlayerLevel { get; set; }

        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(PlayerLevel playerLevel)
            => PlayerLevel = playerLevel;

        public override async ValueTask<IPlayer> CreateAsync()
        {
            var player = await Resolver.ResolveAsync(DIContainer);
            player.Level = PlayerLevel.Value;
            return player;
        }
    }
}