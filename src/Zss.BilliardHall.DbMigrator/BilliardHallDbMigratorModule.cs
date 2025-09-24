using Volo.Abp.Modularity;
using Volo.Abp.Autofac;
using Zss.BilliardHall.EntityFrameworkCore;

namespace Zss.BilliardHall.DbMigrator;

[DependsOn(
    typeof(BilliardHallEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
)]
public class BilliardHallDbMigratorModule : AbpModule
{
    
}