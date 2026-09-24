// CreateWalkInCustomerCommandHandlerTests.cs
using DoubleStar.Modules.Customers.Application.Abstractions;
using DoubleStar.Modules.Customers.Application.Commands.CreateWalkInCustomerCommand;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.Modules.Customers.Tests;

public sealed class CreateWalkInCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _repository = new();

    [Fact]
    public async Task Handle_WhenPhoneAlreadyExists_ReturnsConflict()
    {
        var existing = Customer.CreateWalkIn("Existing", "+2348011112222", null);
        _repository.Setup(r => r.GetByPhoneAsync("+2348011112222", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = new CreateWalkInCustomerCommandHandler(_repository.Object);
        var result = await handler.Handle(new CreateWalkInCustomerCommand("New Person", "+2348011112222", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Customers.PhoneAlreadyInUse");
        _repository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPhoneIsNew_CreatesTheCustomer()
    {
        _repository.Setup(r => r.GetByPhoneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var handler = new CreateWalkInCustomerCommandHandler(_repository.Object);
        var result = await handler.Handle(new CreateWalkInCustomerCommand("New Person", "+2348099998888", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New Person");
        _repository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}