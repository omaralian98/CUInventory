using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Exceptions;
using CUInventory.Procurement.Managers;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CUInventory.Procurement;

public class SupplierManagerTests
{
    private static ContactInfo NewContact(
        string email = "a@b.com",
        string phone = "+963 11 2345678",
        string governorate = "Damascus",
        string city = "Damascus",
        string street = "Al-Mazzeh St")
        => new(new Email(email), new PhoneNumber(phone), new Address(governorate, city, street));

    private static Supplier NewSupplier(string name, ContactInfo contact)
        => new(Guid.NewGuid(), name, contact);

    [Fact]
    public async Task UpdateAsync_Allows_A_Duplicate_Name()
    {
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByEmailOrDefaultAsync(Arg.Any<Email>()).Returns((Supplier?)null);
        repository.GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>()).Returns((Supplier?)null);
        var manager = new SupplierManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        supplier = await manager.UpdateAsync(supplier, "Acme", NewContact(email: "new@b.com", phone: "+963 11 9999999"));

        supplier.Name.ShouldBe("Acme");
    }

    [Fact]
    public async Task UpdateAsync_Skips_The_Uniqueness_Check_When_Contact_Unchanged()
    {
        var repository = Substitute.For<ISupplierRepository>();
        var manager = new SupplierManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        supplier = await manager.UpdateAsync(supplier, "Renamed", NewContact());

        supplier.Name.ShouldBe("Renamed");
        await repository.DidNotReceive().GetByEmailOrDefaultAsync(Arg.Any<Email>());
        await repository.DidNotReceive().GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>());
    }

    [Fact]
    public async Task UpdateAsync_Throws_When_Email_Used_By_Another_Supplier()
    {
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByEmailOrDefaultAsync(new Email("taken@b.com"))
            .Returns(NewSupplier("Other", NewContact(email: "taken@b.com")));
        var manager = new SupplierManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        await Should.ThrowAsync<SupplierEmailAlreadyExistsDomainException>(
            () => manager.UpdateAsync(supplier, "Acme", NewContact(email: "taken@b.com")));
    }

    [Fact]
    public async Task UpdateAsync_Throws_When_PhoneNumber_Used_By_Another_Supplier()
    {
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByEmailOrDefaultAsync(Arg.Any<Email>()).Returns((Supplier?)null);
        repository.GetByPhoneNumberOrDefaultAsync(new PhoneNumber("+963 11 0000000"))
            .Returns(NewSupplier("Other", NewContact(phone: "+963 11 0000000")));
        var manager = new SupplierManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        await Should.ThrowAsync<SupplierPhoneNumberAlreadyExistsDomainException>(
            () => manager.UpdateAsync(supplier, "Acme", NewContact(phone: "+963 11 0000000")));
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Flag_The_Supplier_Against_Itself()
    {
        var repository = Substitute.For<ISupplierRepository>();
        var supplier = NewSupplier("Acme", NewContact());
        // Same contact values are still owned by this very supplier.
        repository.GetByEmailOrDefaultAsync(Arg.Any<Email>()).Returns(supplier);
        repository.GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>()).Returns(supplier);
        var manager = new SupplierManager(repository);

        var updated = await manager.UpdateAsync(supplier, "Acme", NewContact(street: "New St"));

        updated.Contact.Address.Street.ShouldBe("New St");
    }
}
