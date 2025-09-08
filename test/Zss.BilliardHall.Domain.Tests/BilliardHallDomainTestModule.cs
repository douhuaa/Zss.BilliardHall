using Volo.Abp.Modularity;

namespace Zss.BilliardHall;

[DependsOn(typeof(BilliardHallDomainModule), typeof(BilliardHallTestBaseModule))]
public class BilliardHallDomainTestModule : AbpModule
{

}
