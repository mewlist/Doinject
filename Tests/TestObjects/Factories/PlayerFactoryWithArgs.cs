using System.Threading.Tasks;

namespace Doinject.Tests
{
    internal class PlayerFactoryWithArgs : IFactory<int, IPlayer>
    {
        // ReSharper disable once UnusedMember.Global
        public ValueTask<IPlayer> CreateAsync(int arg1)
        {
            var player = arg1 < 10
                ? (IPlayer)new NovicePlayer()
                : new ElderPlayer();
            player.Level = arg1;
            return new ValueTask<IPlayer>(player);
        }
    }
}