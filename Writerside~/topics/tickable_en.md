# Regular Callbacks

You can register callbacks that are called regularly according to the timing of Unity's player loop.
It works for instances managed by the DI container and instances to be injected. It is also guaranteed that the first call will be made after the injection to the target class has been completed.

## Registering Callbacks

By adding the ```[Tickable]``` attribute to any public method, you can register a callback. If you do not specify the timing, it will be called back at the ```Update``` timing.

```C#
public class SomeClass
{
    // Called at Update timing
    [Tickable]
    public void Tick()
    {
        ...
    }
}
```

## Specifying Callback Timing

You can specify the timing of the callback in the arguments of ```Tickable```.

```C#
public class SomeClass
{
    // Called at FixedUpdate timing
    [Tickable(TickableTiming.FixedUpdate)]
    public void Tick()
    {
        ...
    }
}
```

You can specify the following timings for the callback.

* TickableTiming.EarlyUpdate
* TickableTiming.FixedUpdate
* TickableTiming.PreUpdate
* TickableTiming.Update
* TickableTiming.PreLateUpdate
* TickableTiming.PostLateUpdate