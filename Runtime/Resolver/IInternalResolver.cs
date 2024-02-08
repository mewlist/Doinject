using System;
using System.Threading.Tasks;

namespace Doinject
{
    public interface IInternalResolver : IResolver, IAsyncDisposable
    {
        ValueTask<object> ResolveAsObjectAsync(DIContainer container);
        string Name { get; }
        string ShortName { get; }
        string StrategyName { get; }
        int InstanceCount { get; }
    }

    public interface IInternalResolver<T> : IInternalResolver, IResolver<T>
    {
    }
}