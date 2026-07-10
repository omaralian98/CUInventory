using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace CUInventory;


public static class DomainServiceTestExtensions
{
    public static T WithTestGuidGenerator<T>(this T service) where T : DomainService
    {
        var provider = new ServiceCollection()
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .BuildServiceProvider();

        service.LazyServiceProvider = new AbpLazyServiceProvider(provider);
        return service;
    }
}
