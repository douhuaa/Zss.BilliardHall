using Volo.Abp.Modularity;
using Volo.Abp.AutoMapper;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(BilliardHallApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class BilliardHallApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BilliardHallApplicationModule>();
        });
    }
}