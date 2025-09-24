using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(BilliardHallDomainModule),
    typeof(AbpAuthorizationAbstractionsModule)
)]
public class BilliardHallApplicationContractsModule : AbpModule
{
    
}