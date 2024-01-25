using Doinject;
using Doinject.Tests;

public class TestBindingInstallerComponent : BindingInstallerComponent
{
    public override void Install(DIContainer container, IContextArg contextArg)
    {
        base.Install(container, contextArg);
        // Bind your dependencies here
        container.Bind<InjectedObject>();
    }
}
