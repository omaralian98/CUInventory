using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Exceptions;
using CUInventory.Procurement.Managers;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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

    private static ISupplierRepository FreeRepository()
    {
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByEmailOrDefaultAsync(Arg.Any<Email>()).ReturnsNull();
        repository.GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>()).ReturnsNull();
        return repository;
    }

    private static SupplierManager CreateManager(ISupplierRepository repository)
        => new SupplierManager(repository).WithTestGuidGenerator();

    [Fact]
    public async Task CreateAsync_Builds_The_Supplier_When_Contact_Is_Unique()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var contact = NewContact();

        var supplier = await manager.CreateAsync("Acme", contact);

        supplier.ShouldSatisfyAllConditions(
            () => supplier.Name.ShouldBe("Acme"),
            () => supplier.Contact.ShouldBe(contact),
            () => supplier.Id.ShouldNotBe(Guid.Empty));
    }

    [Fact]
    public async Task CreateAsync_Checks_Email_Before_Phone()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);

        await manager.CreateAsync("Acme", NewContact());

        Received.InOrder(() =>
        {
            repository.GetByEmailOrDefaultAsync(Arg.Any<Email>());
            repository.GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>());
        });
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Email_Belongs_To_Another_Supplier()
    {
        var repository = FreeRepository();
        repository.GetByEmailOrDefaultAsync(new Email("taken@b.com"))
            .Returns(NewSupplier("Other", NewContact(email: "taken@b.com")));
        var manager = CreateManager(repository);

        var ex = await Should.ThrowAsync<SupplierEmailAlreadyExistsDomainException>(
            () => manager.CreateAsync("Acme", NewContact(email: "taken@b.com")));

        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.SupplierEmailAlreadyExists),
            () => ex.Data["Email"].ShouldBe("taken@b.com"));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_PhoneNumber_Belongs_To_Another_Supplier()
    {
        var repository = FreeRepository();
        repository.GetByPhoneNumberOrDefaultAsync(new PhoneNumber("+963 11 0000000"))
            .Returns(NewSupplier("Other", NewContact(phone: "+963 11 0000000")));
        var manager = CreateManager(repository);

        var ex = await Should.ThrowAsync<SupplierPhoneNumberAlreadyExistsDomainException>(
            () => manager.CreateAsync("Acme", NewContact(phone: "+963 11 0000000")));

        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.SupplierPhoneNumberAlreadyExists),
            () => ex.Data["PhoneNumber"].ShouldBe("+963 11 0000000"));
    }

    [Fact]
    public async Task UpdateAsync_Allows_A_Duplicate_Name()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        supplier = await manager.UpdateAsync(
            supplier, "Acme", NewContact(email: "new@b.com", phone: "+963 11 9999999"));

        supplier.Name.ShouldBe("Acme");
    }

    [Fact]
    public async Task UpdateAsync_Skips_The_Uniqueness_Check_When_Contact_Unchanged()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        supplier = await manager.UpdateAsync(supplier, "Renamed", NewContact());

        supplier.Name.ShouldBe("Renamed");
        await repository.DidNotReceive().GetByEmailOrDefaultAsync(Arg.Any<Email>());
        await repository.DidNotReceive().GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>());
    }

    [Fact]
    public async Task UpdateAsync_Rechecks_Uniqueness_When_Contact_Changes()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        await manager.UpdateAsync(supplier, "Acme", NewContact(email: "changed@b.com"));

        await repository.Received(1).GetByEmailOrDefaultAsync(Arg.Any<Email>());
        await repository.Received(1).GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>());
    }

    [Fact]
    public async Task UpdateAsync_Throws_When_Email_Belongs_To_Another_Supplier()
    {
        var repository = FreeRepository();
        repository.GetByEmailOrDefaultAsync(new Email("taken@b.com"))
            .Returns(NewSupplier("Other", NewContact(email: "taken@b.com")));
        var manager = CreateManager(repository);
        var supplier = NewSupplier("Acme", NewContact());

        await Should.ThrowAsync<SupplierEmailAlreadyExistsDomainException>(
            () => manager.UpdateAsync(supplier, "Acme", NewContact(email: "taken@b.com")));
    }

    [Fact]
    public async Task UpdateAsync_Does_Not_Flag_The_Supplier_Against_Itself()
    {
        var repository = Substitute.For<ISupplierRepository>();
        var supplier = NewSupplier("Acme", NewContact());
        repository.GetByEmailOrDefaultAsync(Arg.Any<Email>()).Returns(supplier);
        repository.GetByPhoneNumberOrDefaultAsync(Arg.Any<PhoneNumber>()).Returns(supplier);
        var manager = CreateManager(repository);

        var updated = await manager.UpdateAsync(supplier, "Acme", NewContact(street: "New St"));

        updated.Contact.Address.Street.ShouldBe("New St");
    }
}
