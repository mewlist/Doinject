namespace Doinject.Tests
{
    internal class TwoArgsObject : AbstractManyArgsObject
    {
        public TwoArgsObject(Player player1, Player player2)
            => Players.AddRange(new[] { player1, player2 });
    }
}