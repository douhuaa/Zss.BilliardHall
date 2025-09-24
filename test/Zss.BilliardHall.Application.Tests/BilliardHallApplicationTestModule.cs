using Volo.Abp.Modularity;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(BilliardHallApplicationModule),
    typeof(BilliardHallDomainTestModule)
)]
public class BilliardHallApplicationTestModule : AbpModule
{

}
