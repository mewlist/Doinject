using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class FactoryTest
    {
        private DIContainer container;

        [SetUp]
        public void Setup()
        {
            container = new DIContainer(parent: null, SceneManager.GetSceneAt(0));
        }

        [TearDown]
        public async Task TearDown()
        {
            await container.DisposeAsync();
        }

        [Test]
        public async Task PlayerFactoryTest()
        {
            container.Bind<IPlayer>()
                .To<NovicePlayer>()
                .AsFactory();

            var factory = await container.ResolveAsync<IFactory<IPlayer>>();

            var instance = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<NovicePlayer>());
        }

        [Test]
        public async Task InjectablePlayerFactoryTest()
        {
            container.Bind<IPlayer>()
                .To<InjectablePlayer>()
                .Args(5)
                .AsFactory();

            var factory = await container.ResolveAsync<Factory<IPlayer>>();

            var instance = await factory.CreateAsync();
            var instanceB = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<InjectablePlayer>());
            Assert.That(instance.Level, Is.EqualTo(5));
            Assert.That(instance, Is.Not.EqualTo(instanceB));
        }

        [Test]
        public async Task PlayerFactoryWithArgTest()
        {
            container.Bind<IPlayer>()
                .To<InjectablePlayer>()
                .AsFactory<int>();

            var factory = await container.ResolveAsync<Factory<int, IPlayer>>();

            var instance = await factory.CreateAsync(5);
            var instanceB = await factory.CreateAsync(10);
            Assert.That(instance, Is.TypeOf<InjectablePlayer>());
            Assert.That(instance.Level, Is.EqualTo(5));
            Assert.That(instanceB.Level, Is.EqualTo(10));
        }

        [Test]
        public async Task FactoryWith2ArgsTest()
        {
            container.Bind<TwoArgsObject>()
                .AsFactory<Player, Player>();
            var players = new List<Player> { new(), new() };

            var factory = await container.ResolveAsync<Factory<Player, Player, TwoArgsObject>>();
            var instance = await factory.CreateAsync(players[0], players[1]);
            CollectionAssert.AreEqual(instance.Players, players);
        }

        [Test]
        public async Task FactoryWith3ArgsTest()
        {
            container.Bind<ThreeArgsObject>()
                .AsFactory<Player, Player, Player>();
            var players = new List<Player> { new(), new(), new() };

            var factory = await container.ResolveAsync<Factory<Player, Player, Player, ThreeArgsObject>>();
            var instance = await factory.CreateAsync(players[0], players[1], players[2]);
            CollectionAssert.AreEqual(instance.Players, players);
        }

        [Test]
        public async Task FactoryWith4ArgsTest()
        {
            container.Bind<FourArgsObject>()
                .AsFactory<Player, Player, Player, Player>();
            var players = new List<Player> { new(), new(), new(), new() };

            var factory = await container.ResolveAsync<Factory<Player, Player, Player, Player, FourArgsObject>>();
            var instance = await factory.CreateAsync(players[0], players[1], players[2], players[3]);
            CollectionAssert.AreEqual(instance.Players, players);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FourPlayersFactory : Factory<Player, Player, Player, Player, FourArgsObject> { }
        [Test] public async Task InheritedFactoryWith4ArgsTest()
        {
            container.Bind<FourArgsObject>().AsCustomFactory<FourPlayersFactory>();
            var players = new List<Player> { new(), new(), new(), new() };
            var factory = await container.ResolveAsync<FourPlayersFactory>();
            var instance = await factory.CreateAsync(players[0], players[1], players[2], players[3]);
            CollectionAssert.AreEqual(instance.Players, players);
        }

        [Test]
        public void AmbiguousArgsTest()
        {
            Assert.Throws<Exception>(() =>
            {
                container.Bind<IPlayer>()
                    .To<InjectablePlayer>()
                    .Args(99)
                    .AsFactory<int>();
            });
        }

        [Test]
        public async Task MonoBehaviourFactoryTest()
        {
            var gameObject = new GameObject("MonoBehaviourFactoryTest");
            container.Bind<InjectedObject>();
            container.Bind<IInjectableComponent>()
                .To<TestMonoBehaviour>()
                .On(gameObject)
                .AsFactory();

            var factory = await container.ResolveAsync<Factory<IInjectableComponent>>();

            var instance = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<TestMonoBehaviour>());
            Assert.That((instance as TestMonoBehaviour)?.gameObject, Is.EqualTo(gameObject));
        }

        [Test]
        public async Task MonoBehaviourFactoryWithArgsTest()
        {
            var gameObject = new GameObject("MonoBehaviourFactoryWithArgsTest");
            var injectedObject = new InjectedObject();
            container.Bind<IInjectableComponent>()
                .To<TestMonoBehaviour>()
                .On(gameObject)
                .Args(injectedObject)
                .AsFactory();

            var factory = await container.ResolveAsync<Factory<IInjectableComponent>>();

            var instance = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<TestMonoBehaviour>());
            Assert.That((instance as TestMonoBehaviour)?.gameObject, Is.EqualTo(gameObject));
            Assert.That((instance as TestMonoBehaviour)?.InjectedObject, Is.EqualTo(injectedObject));
        }

        [Test]
        public async Task PrefabFactoryTest()
        {
            const string prefabGuid = "b1f3d745bc6e3624b852543a31febb12";
            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<TestMonoBehaviour>(prefabPath);

            container.Bind<InjectedObject>();
            container
                .BindPrefab<TestMonoBehaviour>(prefab)
                .AsFactory();

            var factory = await container.ResolveAsync<Factory<TestMonoBehaviour>>();
            var instance = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<TestMonoBehaviour>());
        }

        [Test]
        public async Task PrefabFactoryWithArgsTest()
        {
            const string prefabGuid = "b1f3d745bc6e3624b852543a31febb12";
            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = AssetDatabase.LoadAssetAtPath<TestMonoBehaviour>(prefabPath);

            var injectedInstance = new InjectedObject();
            container
                .BindPrefab<TestMonoBehaviour>(prefab)
                .Args(injectedInstance)
                .AsFactory();

            var factory = await container.ResolveAsync<Factory<TestMonoBehaviour>>();
            var instance = await factory.CreateAsync();
            Assert.That(instance.InjectedObject, Is.EqualTo(injectedInstance));
        }

        private class PlayerFactory : IFactory<IPlayer>
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

        private class PlayerFactoryWithArgs : IFactory<int, IPlayer>
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

        [TestCase(9)]
        [TestCase(99)]
        public async Task CustomFactoryTest(int playerLevel)
        {
            container.BindFromInstance(new PlayerLevel(playerLevel));
            container.Bind<IPlayer>()
                .AsCustomFactory<PlayerFactory>();

            var factory = await container.ResolveAsync<PlayerFactory>();

            var instance = await factory.CreateAsync();
            if (playerLevel < 10)
                Assert.That(instance, Is.TypeOf<NovicePlayer>());
            else
                Assert.That(instance, Is.TypeOf<ElderPlayer>());
        }

        [TestCase(9)]
        [TestCase(99)]
        public async Task CustomFactoryWithArgsTest(int playerLevel)
        {
            container.BindFromInstance(new PlayerLevel(playerLevel));
            container.Bind<IPlayer>()
                .AsCustomFactory<PlayerFactoryWithArgs>();

            var factory = await container.ResolveAsync<PlayerFactoryWithArgs>();

            var instance = await factory.CreateAsync(playerLevel);
            if (playerLevel < 10)
                Assert.That(instance, Is.TypeOf<NovicePlayer>());
            else
                Assert.That(instance, Is.TypeOf<ElderPlayer>());
        }


        internal interface ISomeApi
        {
            public ValueTask<Player> GetPlayerAsync();
        }

        private class SomeApi : ISomeApi
        {
            public async ValueTask<Player> GetPlayerAsync()
            {
                await Task.Delay(100);
                return new Player { Level = 55 };
            }
        }

        private class AsyncPlayerFactory : IFactory<IPlayer>
        {
            private ISomeApi Api { get; set; }

            [Inject] public void Construct(ISomeApi someApi)
            {
                Api = someApi;
            }

            public async ValueTask<IPlayer> CreateAsync()
            {
                var player = await Api.GetPlayerAsync();
                return player;
            }
        }

        [Test]
        public async Task AsyncCustomFactoryTest()
        {
            container.Bind<ISomeApi, SomeApi>();
            container.Bind<IPlayer>()
                .AsCustomFactory<AsyncPlayerFactory>();

            var factory = await container.ResolveAsync<AsyncPlayerFactory>();
            var instance = await factory.CreateAsync();
            Assert.That(instance.Level, Is.EqualTo(55));
        }

        public class WithPlayerResolverFactory : Factory<IPlayer>
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

        [TestCase(9)]
        [TestCase(99)]
        public async Task InheritedFactoryTest(int playerLevel)
        {
            container.BindFromInstance(new PlayerLevel(playerLevel));
            container.Bind<IPlayer>()
                .To<ElderPlayer>()
                .AsCustomFactory<WithPlayerResolverFactory>();

            var factory = await container.ResolveAsync<WithPlayerResolverFactory>();

            var instance = await factory.CreateAsync();
            Assert.That(instance, Is.TypeOf<ElderPlayer>());
            Assert.That(instance.Level, Is.EqualTo(playerLevel));
        }

        public sealed class PlayerLevel
        {
            public int Value { get; set; }
            public PlayerLevel(int level) { Value = level; }
        }

        public interface IPlayer
        {
            int Level { get; set; }
        }

        public class Player : IPlayer
        {
            public int Level { get; set; }
        }

        private class InjectablePlayer : IPlayer
        {
            public int Level { get; set; }

            public InjectablePlayer(int level)
            {
                Level = level;
            }
        }

        public class AbstractManyArgsObject
        {
            public List<Player> Players { get; } = new();
        }
        private class TwoArgsObject : AbstractManyArgsObject
        {
            public TwoArgsObject(Player player1, Player player2)
                => Players.AddRange(new[] { player1, player2 });
        }
        private class ThreeArgsObject : AbstractManyArgsObject
        {
            public ThreeArgsObject(Player player1, Player player2, Player player3)
                => Players.AddRange(new[] { player1, player2, player3 });
        }

        public class FourArgsObject : AbstractManyArgsObject
        {
            public FourArgsObject(Player player1, Player player2, Player player3, Player player4)
                => Players.AddRange(new[] { player1, player2, player3, player4 });
        }

        private sealed class NovicePlayer : Player { }
        private sealed class ElderPlayer : Player { }
    }
}