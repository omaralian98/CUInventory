using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Interfaces;
using CUInventory.Catalog.Repositories;

namespace CUInventory.Catalog.Managers;

public class CategoryManager(ICategoryRepository categoryRepository) : DomainService, ICategoryManager
{
    public async Task<Category> CreateAsync(string name)
    {
        var exists = await categoryRepository.ExistsAsync(name);
        if (exists)
        {
            //todo throw domain exception
        }

        var category = new Category(GuidGenerator.Create(), name);
        return category;
    }

    public async Task<Category> UpdateAsync(Category category, string name, int orderIndex, bool isActive)
    {
        if (string.Equals(category.Name, name) == false)
        {
            var exists = await categoryRepository.ExistsAsync(name);
            if (exists)
            {
                //todo throw domain exception
            }
        }

        category.OrderIndex = orderIndex;
        category.SetIsActive(isActive);
        return category;
    }
}
