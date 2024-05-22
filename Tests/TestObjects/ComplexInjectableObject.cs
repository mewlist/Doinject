namespace Doinject.Tests
{
    internal class ComplexInjectableObject
    {
        [Inject] public InjectedObject InjectedObjectA { get; set; }
        [Inject] public InjectedObject InjectedObjectB { get; set; }
        [Inject] public Player PlayerFromProperty { get; set; }
        [Inject] public NovicePlayer NovicePlayerFromProperty { get; set; }

        public Player PlayerFromMethod1 { get; set; }
        public Player PlayerFromMethod2 { get; set; }
        public NovicePlayer NovicePlayerFromMethod1 { get; set; }
        public NovicePlayer NovicePlayerFromMethod2 { get; set; }

        [Inject]
        public void ConstructA(Player player, NovicePlayer novicePlayer)
        {
            PlayerFromMethod1 = player;
            NovicePlayerFromMethod1 = novicePlayer;
        }

        [Inject]
        public void Construct(Player player, NovicePlayer novicePlayer)
        {
            PlayerFromMethod2 = player;
            NovicePlayerFromMethod2 = novicePlayer;
        }
    }
}