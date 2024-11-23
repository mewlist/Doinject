using System;
using System.Threading.Tasks;
using Mew.Core.TaskHelpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doinject.Tests
{
    public class TickableTest
    {
        private DIContainer container;

        public int TargetFrameRate { get; set; }

        [SetUp]
        public void Setup()
        {
            TargetFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 10;
            var scene = SceneManager.GetActiveScene();
            container = new DIContainer(parent: null, scene);
        }


        [TearDown]
        public async Task TearDown()
        {
            await container.DisposeAsync();
            Application.targetFrameRate = TargetFrameRate;
        }

        [Test]
        public async Task UpdateTimingTest()
        {
            var tickable = new TickableObject();
            container.BindFromInstance(tickable);
            var instance = await container.ResolveAsync<TickableObject>();
            await TaskHelperInternal.NextFrame();
            instance.CountEnabled = true;
            for (var i = 0; i < 10; i++)
                await TaskHelperInternal.NextFrame();
            await TaskHelperInternal.NextFrame();
            await TaskHelperInternal.NextFrame();
            instance.CountEnabled = false;
            Assert.That(instance.EarlyUpdateCount, Is.GreaterThan(8));
            Assert.That(instance.FixedUpdateCount, Is.GreaterThan(8));
            Assert.That(instance.PreUpdateCount, Is.GreaterThan(8));
            Assert.That(instance.UpdateCount, Is.GreaterThan(8));
            Assert.That(instance.PreLateUpdateCount, Is.GreaterThan(8));
            Assert.That(instance.PostLateUpdateCount, Is.GreaterThan(8));
            Assert.That(instance.FixedUpdateCount, Is.GreaterThan(instance.UpdateCount));
        }

        [Test]
        public async Task InvalidTickableTest()
        {
            using var exceptionTestScope = new ExceptionTestScope();

            try
            {
                container.Bind<InvalidTickableObject>();
                var _ = await container.ResolveAsync<InvalidTickableObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Tickable method should not have any parameters"))
                    Assert.Pass();
            }
            Assert.Fail();
        }
    }
}