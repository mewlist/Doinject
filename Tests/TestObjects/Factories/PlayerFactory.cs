using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class PlayerFactory : IFactory<IPlayer>
    {
        private PlayerLevel PlayerLevel { get; set; }
        // ReSharper disable once UnusedMember.Global
        [Inject] public void Construct(PlayerLevel playerLevel)
        {
            PlayerLevel = playerLevel;
        }
        public ValueTask<IPlayer> CreateAsync()
        {
            var player = PlayerLevel.Value < 10
                ? (IPlayer)new NovicePlayer()
                : new ElderPlayer();
            player.Level = PlayerLevel.Value;
            return new ValueTask<IPlayer>(player);
        }
    }
}