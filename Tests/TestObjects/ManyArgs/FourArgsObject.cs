namespace Doinject.Tests
{
    internal class FourArgsObject : AbstractManyArgsObject
    {
        public FourArgsObject(Player player1, Player player2, Player player3, Player player4)
            => Players.AddRange(new[] { player1, player2, player3, player4 });
    }
}