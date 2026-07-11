using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace CUInventory;


public static class DomainServiceTestExtensions
{
    public static readonly DateTime TestNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static T WithTestGuidGenerator<T>(this T service) where T : DomainService
    {
        var provider = new ServiceCollection()
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .AddSingleton<IClock>(new TestClock())
            .BuildServiceProvider();

        service.LazyServiceProvider = new AbpLazyServiceProvider(provider);
        return service;
    }

    private sealed class TestClock : IClock
    {
        public DateTime Now => TestNow;
        public DateTimeKind Kind => DateTimeKind.Utc;
        public bool SupportsMultipleTimezone => false;
        public DateTime Normalize(DateTime dateTime) => dateTime;
        public DateTime ConvertToUserTime(DateTime dateTime) => dateTime;
        public DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset) => dateTimeOffset;
        public DateTime ConvertToUtc(DateTime dateTime) => dateTime;
    }
}
