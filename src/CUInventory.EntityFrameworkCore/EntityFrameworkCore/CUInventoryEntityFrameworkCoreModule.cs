using System;
using CUInventory.Catalog.Aggregates;
using CUInventory.EntityFrameworkCore.Repositories.Catalog;
using CUInventory.EntityFrameworkCore.Repositories.Inventory;
using CUInventory.EntityFrameworkCore.Repositories.Procurement;
using CUInventory.EntityFrameworkCore.Repositories.Sales;
using CUInventory.EntityFrameworkCore.Repositories.Warehousing;
using CUInventory.Inventory.Aggregates;
using CUInventory.Procurement.Aggregates;
using CUInventory.Sales.Aggregates;
using CUInventory.Warehousing.Aggregates;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Volo.Abp.Studio;

namespace CUInventory.EntityFrameworkCore;

[DependsOn(
    typeof(CUInventoryDomainModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
    )]
public class CUInventoryEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {

        CUInventoryEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<CUInventoryDbContext>(options =>
        {
                /* Default repositories for aggregate roots only. Child entities
                 * (SaleLine, StockTransferLine, etc.) are intentionally left without
                 * a repository so they can only be reached through their aggregate root. */
            options.AddDefaultRepositories(includeAllEntities: false);

            // Custom repositories per aggregate root. AddRepository binds both the default
            // IRepository<T,Guid> and the aggregate's custom repository interface.
            options.AddRepository<Category, EfCoreCategoryRepository>();
            options.AddRepository<Product, EfCoreProductRepository>();
            options.AddRepository<Supplier, EfCoreSupplierRepository>();
            options.AddRepository<PurchaseOrder, EfCorePurchaseOrderRepository>();
            options.AddRepository<Warehouse, EfCoreWarehouseRepository>();
            options.AddRepository<Shipment, EfCoreShipmentRepository>();
            options.AddRepository<InventoryBalance, EfCoreInventoryBalanceRepository>();
            options.AddRepository<InventoryLot, EfCoreInventoryLotRepository>();
            options.AddRepository<StockTransfer, EfCoreStockTransferRepository>();
            options.AddRepository<Sale, EfCoreSaleRepository>();
        });

        if (AbpStudioAnalyzeHelper.IsInAnalyzeMode)
        {
            return;
        }

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also CUInventoryDbContextFactory for EF Core tooling. */

            options.UseSqlServer();

        });
        
    }
}
