using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.EventHandlers;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Tests;

public sealed class CustomerAccountRegisteredEventHandlerTests
{
    private readonly Mock<ICustomerRepository> _repository = new();

    [Fact]
    public async Task Handle_WhenCustomerAlreadyLinked_DoesNotAddAgain()
    {
        var userId = Guid.NewGuid();

        _repository
            .Setup(r => r.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Customer.CreateForAccount(
                    userId,
                    "ada@example.com"));

        var handler = new CustomerAccountRegisteredEventHandler(
            _repository.Object);

        await handler.Handle(
            new CustomerAccountRegisteredEvent(
                userId,
                "ada@example.com"),
            CancellationToken.None);

        _repository.Verify(
            r => r.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotYetLinked_CreatesCustomerWithMatchingId()
    {
        var userId = Guid.NewGuid();

        _repository
            .Setup(r => r.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new CustomerAccountRegisteredEventHandler(
            _repository.Object);

        await handler.Handle(
            new CustomerAccountRegisteredEvent(
                userId,
                "ada@example.com"),
            CancellationToken.None);

        _repository.Verify(
            r => r.AddAsync(
                It.Is<Customer>(
                    c =>
                        c.Id == userId &&
                        c.Email == "ada@example.com" &&
                        c.Name == null &&
                        c.Phone == null &&
                        c.HasAccount),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}