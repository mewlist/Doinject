namespace Doinject.Tests
{
    internal class TickableObject
    {
        public int EarlyUpdateCount { get; private set; }
        public int FixedUpdateCount { get; private set; }
        public int PreUpdateCount { get; private set; }
        public int UpdateCount { get; private set; }
        public int PreLateUpdateCount { get; private set; }
        public int PostLateUpdateCount { get; private set; }
        public bool CountEnabled { get; set; }

        [Tickable(TickableTiming.EarlyUpdate)] public void EarlyUpdate()
        {
            if (CountEnabled) EarlyUpdateCount++;
        }

        [Tickable(TickableTiming.FixedUpdate)] public void FixedUpdate()
        {
            if (CountEnabled) FixedUpdateCount++;
        }

        [Tickable(TickableTiming.PreUpdate)] public void PreUpdate()
        {
            if (CountEnabled) PreUpdateCount++;
        }

        [Tickable(TickableTiming.Update)] public void Update()
        {
            if (CountEnabled) UpdateCount++;
        }

        [Tickable(TickableTiming.PreLateUpdate)] public void PreLateUpdate()
        {
            if (CountEnabled) PreLateUpdateCount++;
        }

        [Tickable(TickableTiming.PostLateUpdate)] public void PostLateUpdate()
        {
            if (CountEnabled) PostLateUpdateCount++;
        }
    }

    internal class InvalidTickableObject
    {
        [Tickable]
        public void PostLateUpdate(int arg)
        {
        }
    }
}