using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zss.BilliardHall.Data;
using Volo.Abp.DependencyInjection;

namespace Zss.BilliardHall.EntityFrameworkCore;

public class EntityFrameworkCoreBilliardHallDbSchemaMigrator
    : IBilliardHallDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreBilliardHallDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the BilliardHallDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<BilliardHallDbContext>()
            .Database
            .MigrateAsync();
    }
}
