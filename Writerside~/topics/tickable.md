# 定期的なコールバック

Unity のプレイヤーループタイミングに従って、定期的に呼び出されるコールバックを登録できます。
DI コンテナにより管理されるインスタンスや、注入対象となるインスタンスに対して機能します。
また、対象のクラスへの注入が完了した以降に初回の呼び出しが行われることが保証されます。

## コールバックの登録

任意の public なメソッドに ```[Tickable]``` 属性をつけることで、コールバックを登録できます。
タイミングを指定しなかった場合は、```Update``` タイミングでコールバックされます。

```C#
public class SomeClass
{
    // Update タイミングで呼び出される
    [Tickable]
    public void Tick()
    {
        ...
    }
}
```

## コールバックタイミングの指定

```Tickable```　の引数にコールバックのタイミングを指定できます。

```C#
public class SomeClass
{
    // FixedUpdate タイミングで呼び出される
    [Tickable(TickableTiming.FixedUpdate)]
    public void Tick()
    {
        ...
    }
}
```

コールバックには以下のタイミングを指定できます。

* TickableTiming.EarlyUpdate
* TickableTiming.FixedUpdate
* TickableTiming.PreUpdate
* TickableTiming.Update
* TickableTiming.PreLateUpdate
* TickableTiming.PostLateUpdate
