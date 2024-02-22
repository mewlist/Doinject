using System;

namespace Doinject
{
    public enum TickableTiming
    {
        EarlyUpdate,
        FixedUpdate,
        PreUpdate,
        Update,
        PreLateUpdate,
        PostLateUpdate,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TickableAttribute : Attribute
    {
        public TickableTiming Timing { get; set; }

        public TickableAttribute(TickableTiming timing = TickableTiming.Update)
        {
            Timing = timing;
        }
    }
}