using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Interfaces;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace CUInventory.Catalog;

public class CatalogDataSeedContributor(
    ICategoryManager categoryManager,
    IProductManager productManager,
    ICategoryRepository categoryRepository,
    IProductRepository productRepository)
    : IDataSeedContributor, ITransientDependency
{
    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var categoryNames = new[]
        {
            "Electronics", "Office Supplies", "Furniture", "Hardware", "Cleaning Supplies",
            "Kitchen & Break Room", "Safety Equipment", "Networking", "Printing & Paper", "Storage & Shelving",
        };

        var categories = new Dictionary<string, Category>();
        foreach (var name in categoryNames)
        {
            categories[name] = await GetOrCreateCategoryAsync(name);
        }

        var products = new List<(string Name, string? Description, string? Sku, string Category, bool IsService)>
        {
            ("Wireless Mouse", "2.4GHz optical wireless mouse", "ELEC-MOUSE-001", "Electronics", false),
            ("USB-C Charger", "65W GaN fast charger", "ELEC-CHRG-001", "Electronics", false),
            ("Mechanical Keyboard", "Tenkeyless, brown switches", "ELEC-KBD-001", "Electronics", false),
            ("Ballpoint Pen (Pack of 10)", "Blue ink, medium tip", "OFF-PEN-010", "Office Supplies", false),
            ("A4 Paper Ream", "500 sheets, 80gsm", "OFF-PAPER-A4", "Office Supplies", false),
            ("Ergonomic Office Chair", "Adjustable lumbar support", "FURN-CHAIR-001", "Furniture", false),
            ("Standing Desk", "Electric height-adjustable desk", "FURN-DESK-001", "Furniture", false),
            ("Cordless Drill", "18V brushless drill/driver", "HW-DRILL-001", "Hardware", false),
            ("Screwdriver Set (32pc)", "Precision + standard bits", "HW-SDSET-032", "Hardware", false),
            ("All-Purpose Cleaner (5L)", "Concentrated multi-surface", "CLN-APC-005", "Cleaning Supplies", false),
            ("Coffee Machine", "Bean-to-cup, 1.8L", "KIT-COFFEE-001", "Kitchen & Break Room", false),
            ("Safety Helmet", "Hard hat, EN 397 certified", "SAF-HELM-001", "Safety Equipment", false),
            ("Gigabit Switch (8-port)", "Unmanaged desktop switch", "NET-SW-008", "Networking", false),
            ("Furniture Assembly Service", "On-site assembly, per hour", null, "Furniture", true),
            ("Equipment Installation Service", "On-site installation, per hour", null, "Hardware", true),
        };

        foreach (var product in products)
        {
            try
            {
                await SeedProductAsync(
                    product.Name,
                    product.Description,
                    product.Sku,
                    categories[product.Category].Id,
                    product.IsService
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task<Category> GetOrCreateCategoryAsync(string name)
    {
        var existing = await categoryRepository.FindAsync(c => c.Name == name);
        if (existing is not null)
        {
            return existing;
        }

        var category = await categoryManager.CreateAsync(name);
        return await categoryRepository.InsertAsync(category, autoSave: true);
    }

    private async Task SeedProductAsync(
        string name,
        string? description,
        string? sku,
        Guid? categoryId,
        bool isService)
    {
        var skuValue = sku is null ? null : new Sku(sku);

        // Idempotent: skip if the product already exists. SKU-bearing products are matched by SKU;
        // SKU-less products (e.g. services) are matched by name so re-runs don't duplicate them.
        var existing = skuValue is null
            ? await productRepository.FindAsync(p => p.Name == name)
            : await productRepository.GetProductBySkuOrDefaultAsync(skuValue);
        if (existing is not null)
        {
            return;
        }

        var product = await productManager.CreateAsync(name, description, skuValue, isService, categoryId);
        await productRepository.InsertAsync(product, autoSave: true);
    }
}
