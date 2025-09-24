using Volo.Abp.Modularity;

namespace Zss.BilliardHall;

public abstract class BilliardHallApplicationTestBase<TStartupModule> : BilliardHallTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
