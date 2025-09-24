using Volo.Abp.Modularity;

namespace Zss.BilliardHall;

/* Inherit from this class for your domain layer tests. */
public abstract class BilliardHallDomainTestBase<TStartupModule> : BilliardHallTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
