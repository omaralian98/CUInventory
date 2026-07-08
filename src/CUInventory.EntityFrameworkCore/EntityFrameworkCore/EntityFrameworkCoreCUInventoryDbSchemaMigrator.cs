using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CUInventory.Data;
using Volo.Abp.DependencyInjection;

namespace CUInventory.EntityFrameworkCore;

public class EntityFrameworkCoreCUInventoryDbSchemaMigrator
    : ICUInventoryDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreCUInventoryDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the CUInventoryDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<CUInventoryDbContext>()
            .Database
            .MigrateAsync();
    }
}
