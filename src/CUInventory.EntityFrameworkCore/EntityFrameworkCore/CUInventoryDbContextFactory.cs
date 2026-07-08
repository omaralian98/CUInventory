using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CUInventory.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class CUInventoryDbContextFactory : IDesignTimeDbContextFactory<CUInventoryDbContext>
{
    public CUInventoryDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        CUInventoryEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<CUInventoryDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new CUInventoryDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CUInventory.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
