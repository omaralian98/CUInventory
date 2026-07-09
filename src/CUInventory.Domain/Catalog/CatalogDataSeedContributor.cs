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
        var electronics = await GetOrCreateCategoryAsync("Electronics");
        var officeSupplies = await GetOrCreateCategoryAsync("Office Supplies");
        var furniture = await GetOrCreateCategoryAsync("Furniture");

        var products = new List<(string Name, string? Description, string? Sku, Guid? CategoryId, bool IsService)>
        {
            ("Wireless Mouse", "2.4GHz optical wireless mouse", "ELEC-MOUSE-001", electronics.Id, false),
            ("USB-C Charger", "65W GaN fast charger", "ELEC-CHRG-001", electronics.Id, false),
            ("Ballpoint Pen (Pack of 10)", "Blue ink, medium tip", "OFF-PEN-010", officeSupplies.Id, false),
            ("A4 Paper Ream", "500 sheets, 80gsm", "OFF-PAPER-A4", officeSupplies.Id, false),
            ("Ergonomic Office Chair", "Adjustable lumbar support", "FURN-CHAIR-001", furniture.Id, false),
            ("Standing Desk", "Electric height-adjustable desk", "FURN-DESK-001", furniture.Id, false),
            ("Furniture Assembly Service", "On-site assembly, per hour", null, furniture.Id, true),
        };

        foreach (var product in products)
        {
            try
            {
                await SeedProductAsync(
                    product.Name,
                    product.Description,
                    product.Sku,
                    product.CategoryId,
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


        var product = await productManager.CreateAsync(name, description, skuValue, isService, categoryId);
        await productRepository.InsertAsync(product, autoSave: true);
    }
}
