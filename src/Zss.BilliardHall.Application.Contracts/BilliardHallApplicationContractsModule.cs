using Volo.Abp.Modularity;
using Volo.Abp.Ddd.Application.Contracts;
using Volo.Abp.Authorization;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(BilliardHallDomainModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationAbstractionsModule)
)]
public class BilliardHallApplicationContractsModule : AbpModule
{
    
}