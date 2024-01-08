using System.Threading.Tasks;

namespace Doinject
{
    public interface ICacheStrategy
    {
        CacheStrategy CacheStrategy { get; }
        Task TryCacheAsync(DIContainer container);
    }
}