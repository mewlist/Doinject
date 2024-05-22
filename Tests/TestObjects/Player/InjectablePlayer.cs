namespace Doinject.Tests
{
    internal class InjectablePlayer : IPlayer
    {
        public int Level { get; set; }

        public InjectablePlayer(int level)
        {
            Level = level;
        }
    }
}