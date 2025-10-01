using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Zss.BilliardHall.Data;

/* This is used if database provider does't define
 * IBilliardHallDbSchemaMigrator implementation.
 */
public class NullBilliardHallDbSchemaMigrator : IBilliardHallDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
