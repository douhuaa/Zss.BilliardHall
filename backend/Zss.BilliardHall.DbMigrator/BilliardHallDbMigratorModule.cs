using Zss.BilliardHall.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Zss.BilliardHall.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BilliardHallEntityFrameworkCoreModule),
    typeof(BilliardHallApplicationContractsModule)
)]
public class BilliardHallDbMigratorModule : AbpModule
{
}
