using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;

namespace Zss.BilliardHall.DbMigrator;

public class BilliardHallDbMigrationService : ITransientDependency
{
    private readonly ILogger<BilliardHallDbMigrationService> _logger;
    private readonly IDataSeeder _dataSeeder;

    public BilliardHallDbMigrationService(
        ILogger<BilliardHallDbMigrationService> logger,
        IDataSeeder dataSeeder)
    {
        _logger = logger;
        _dataSeeder = dataSeeder;
    }

    public async Task MigrateAsync()
    {
        _logger.LogInformation("开始数据库迁移...");
        
        await _dataSeeder.SeedAsync();
        
        _logger.LogInformation("数据库迁移成功完成！");
    }
}