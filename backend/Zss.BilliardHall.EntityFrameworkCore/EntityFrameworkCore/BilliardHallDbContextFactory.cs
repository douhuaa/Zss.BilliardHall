using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Zss.BilliardHall.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class BilliardHallDbContextFactory : IDesignTimeDbContextFactory<BilliardHallDbContext>
{
    public BilliardHallDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        BilliardHallEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<BilliardHallDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));
        
        return new BilliardHallDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Zss.BilliardHall.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
