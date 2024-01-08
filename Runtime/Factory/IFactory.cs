using System.Threading.Tasks;

namespace Doinject
{
    public interface IFactory
    {
    }

    public interface IFactory<T> : IFactory
    {
        ValueTask<T> CreateAsync();
    }

    public interface IFactory<in TArg1, T> : IFactory
    {
        ValueTask<T> CreateAsync(TArg1 arg1);
    }

    public interface IFactory<in TArg1, in TArg2, T> : IFactory
    {
        ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2);
    }

    public interface IFactory<in TArg1, in TArg2, in TArg3, T> : IFactory
    {
        ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3);
    }

    public interface IFactory<in TArg1, in TArg2, in TArg3, in TArg4, T> : IFactory
    {
        ValueTask<T> CreateAsync(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
    }
}