namespace Doinject.Tests
{
    internal class Player : IPlayer
    {
        public PlayerId PlayerId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
    }
}