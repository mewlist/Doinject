# Advanced Bindings

## Custom Factories

When the instance creation process is complex due to the factory, there are cases where it cannot be handled by coding bindings alone.
For example, if you have a factory that involves API communication, you may need to communicate and use the results to generate instances.

In such cases, you can use a custom factory to code the factory's creation process.

Assume that ISomeApi is already bound to the DI container.

```C#

internal interface ISomeApi
{
    public ValueTask<Player> GetPlayerAsync();
}
```

A factory that uses the results of API communication to create ```IPlayer``` can be written by inheriting ```IFactory<T>``` as follows.

```C#
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
```

Inject the necessary instances with ```[Inject] Construct(...)``` and describe the process of creating instances in ```CreateAsync()```.

To bind a custom factory, specify the type of the custom factory for ```AsCustomFactory<T>``` as follows.

```C#
container.Bind<IPlayer>()
    .AsCustomFactory<AsyncPlayerFactory>();
```

This binds ```AsyncPlayerFactory``` to the DI container.

## Custom Resolvers

You can also customize the way the DI container creates instances.
For example, a resolver that retrieves ```IPlayer``` from a repository can be written by inheriting ```IResolver<T>``` as follows.

```C#
public class CustomResolver : IResolver<IPlayer>
{
    private PlayerId Id { get; set; }
    private IPlayerRepository PlayerRepository { get; set; }

    [Inject] public void Construct(PlayerId id, IPlayerRepository playerRepository)
    {
        Id = id;
        PlayerRepository = playerRepository;
    }

    public async ValueTask<IPlayer> ResolveAsync(IReadOnlyDIContainer container, object[] args)
    {
        return await PlayerRepository.Get(Id);
    }
}
```

Like a custom factory, you can inject the necessary instances and generate instances in the way you want.
The ```args``` argument to ```ResolveAsync``` is passed as an array of arguments specified in ```Args(...)``` in the binding coding.

To bind using a custom resolver, specify ```FromResolver<T>``` as follows.

```C#
container.Bind<IPlayer>()
    .FromResolver<CustomResolver>();
```

## Passing Arguments through Factories

If a constructor or injection point method requires arguments, specify the type of the argument and call ```AsFactory<TArg1, ...>()```.

Consider a type with a constructor argument as follows.

```C#
private class SomeObject
{
    public SomeObject(int someArg)
    {
        ...
    }
}
```

In the case of a class with arguments in the constructor like this, specify the int type as follows.

```C#
container.Bind<SomeObject>().AsFactory<int>()
```

To pass an argument, pass it as an argument to ```CreateAsync()```.

```C#
[Inject] public void Construct(IFactory<SomeObject> factory)
{
    var someObject = await factory.CreateAsync(123);
}
```

> The argument specification to AsFactory supports up to four and the following interfaces are defined.
>
> ```C#
> IFactory<TArg1, T>
> IFactory<TArg1, TArg2, T>
> IFactory<TArg1, TArg2, TArg3, T>
> IFactory<TArg1, TArg2, TArg3, TArg4, T>
> ```
{style="note"}

### Passing Arguments through Custom Factories

Depending on the number of arguments, define a custom factory by inheriting IFactory<TArg1, ... T>.

```C#
private class SomeFactoryWithArgs : IFactory<int, IPlayer>
{
    public ValueTask<IPlayer> CreateAsync(int playerLevel)
    {
        ...
    }
}
```