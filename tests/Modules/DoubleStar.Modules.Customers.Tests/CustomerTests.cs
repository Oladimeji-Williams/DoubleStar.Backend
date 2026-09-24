// CustomerTests.cs
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Tests;

public sealed class CustomerTests
{
    [Fact]
    public void CreateWalkIn_HasAccountIsFalse()
    {
        var customer = Customer.CreateWalkIn("Ngozi Chukwu", "+2348011112222", null);
        customer.HasAccount.Should().BeFalse();
        customer.Name.Should().Be("Ngozi Chukwu");
    }

    [Fact]
    public void CreateForAccount_UsesTheGivenUserIdAsItsOwnId()
    {
        var userId = Guid.NewGuid();
        var customer = Customer.CreateForAccount(userId, "Ada Okafor", null, "ada@example.com");

        customer.Id.Should().Be(userId);
        customer.HasAccount.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_TrimsWhitespaceOnEveryField()
    {
        var customer = Customer.CreateWalkIn("Ada", null, null);
        customer.UpdateDetails("  Ada Okafor  ", " +234801 ", " ada@example.com ", " Lekki ");

        customer.Name.Should().Be("Ada Okafor");
        customer.Phone.Should().Be("+234801");
        customer.Email.Should().Be("ada@example.com");
        customer.Address.Should().Be("Lekki");
    }
}