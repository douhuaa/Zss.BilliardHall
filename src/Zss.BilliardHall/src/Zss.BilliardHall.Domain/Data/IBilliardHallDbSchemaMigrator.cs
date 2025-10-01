using System.Threading.Tasks;

namespace Zss.BilliardHall.Data;

public interface IBilliardHallDbSchemaMigrator
{
    Task MigrateAsync();
}
