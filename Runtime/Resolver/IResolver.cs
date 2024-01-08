using System.Threading.Tasks;

namespace Doinject
{
    public interface IResolver
    {
    }

    public interface IResolver<T> : IResolver
    {
        ValueTask<T> ResolveAsync(IReadOnlyDIContainer container, object[] args = null);
    }
}