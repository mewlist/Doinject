namespace Doinject.Tests
{
    internal class ThreeArgsObject : AbstractManyArgsObject
    {
        public ThreeArgsObject(Player player1, Player player2, Player player3)
            => Players.AddRange(new[] { player1, player2, player3 });
    }
}