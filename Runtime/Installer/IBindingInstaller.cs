
namespace Doinject
{
    public interface IBindingInstaller
    {
        void Install(DIContainer container, IContextArg contextArg);
    }
}