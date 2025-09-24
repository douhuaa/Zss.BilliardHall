using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(BilliardHallApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class BilliardHallHttpApiModule : AbpModule
{
    
}