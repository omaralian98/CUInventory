using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Dtos;
using CUInventory.Shared.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Procurement;

public abstract class SupplierAppServiceTests<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISupplierAppService _supplierAppService;

    protected SupplierAppServiceTests()
    {
        _supplierAppService = GetRequiredService<ISupplierAppService>();
    }

    private static ContactInfoDto Contact(string email, string phone = "+963112345678") => new()
    {
        Email = email,
        PhoneNumber = phone,
        Address = new AddressDto { Governorate = "Damascus", City = "Damascus", Street = "Al-Mazzeh St" }
    };

    [Fact]
    public async Task Should_Create_Get_Update_And_Delete_A_Supplier()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var email = $"acme{token}@example.com";

        var created = await _supplierAppService.CreateAsync(new CreateSupplierDto
        {
            Name = "Acme",
            Contact = Contact(email)
        });

        created.ShouldSatisfyAllConditions(
            () => created.Id.ShouldNotBe(Guid.Empty),
            () => created.Name.ShouldBe("Acme"),
            // The Email value object normalizes to lower-invariant.
            () => created.Contact.Email.ShouldBe(email.ToLowerInvariant()),
            () => created.Contact.PhoneNumber.ShouldBe("+963112345678"),
            () => created.Contact.Address.City.ShouldBe("Damascus"));

        var fetched = await _supplierAppService.GetAsync(created.Id);

        var updated = await _supplierAppService.UpdateAsync(created.Id, new UpdateSupplierDto
        {
            Name = "Acme Renamed",
            Contact = Contact($"renamed{token}@example.com", "+963119999999"),
            ConcurrencyStamp = fetched.ConcurrencyStamp
        });

        updated.ShouldSatisfyAllConditions(
            () => updated.Name.ShouldBe("Acme Renamed"),
            () => updated.Contact.Email.ShouldBe($"renamed{token}@example.com"),
            () => updated.Contact.PhoneNumber.ShouldBe("+963119999999"));

        await _supplierAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _supplierAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Reject_A_Duplicate_Email()
    {
        var email = $"dup{Guid.NewGuid():N}@example.com";
        await _supplierAppService.CreateAsync(new CreateSupplierDto { Name = "First", Contact = Contact(email) });

        await Should.ThrowAsync<BusinessException>(
            () => _supplierAppService.CreateAsync(new CreateSupplierDto
            {
                Name = "Second",
                Contact = Contact(email, "+963110000000")
            }));
    }

    [Fact]
    public async Task Should_Reject_A_Duplicate_Phone_Number()
    {
        var phone = "+963 11 " + new Random().Next(1000000, 9999999);
        await _supplierAppService.CreateAsync(new CreateSupplierDto
        {
            Name = "First",
            Contact = Contact($"a{Guid.NewGuid():N}@example.com", phone)
        });

        await Should.ThrowAsync<BusinessException>(
            () => _supplierAppService.CreateAsync(new CreateSupplierDto
            {
                Name = "Second",
                Contact = Contact($"b{Guid.NewGuid():N}@example.com", phone)
            }));
    }

    [Fact]
    public async Task Should_Filter_By_Name_Or_Email()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var created = await _supplierAppService.CreateAsync(new CreateSupplierDto
        {
            Name = $"Named-{token}",
            Contact = Contact($"filter{token}@example.com")
        });

        var byName = await _supplierAppService.GetListAsync(new GetSupplierListDto { Filter = $"Named-{token}" });
        byName.Items.ShouldContain(s => s.Id == created.Id);

        var byEmail = await _supplierAppService.GetListAsync(new GetSupplierListDto { Filter = $"filter{token}" });
        byEmail.Items.ShouldContain(s => s.Id == created.Id);
    }
}
