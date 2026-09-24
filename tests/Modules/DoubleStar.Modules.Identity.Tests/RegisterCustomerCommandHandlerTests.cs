// RegisterCustomerCommandHandlerTests.cs
using DoubleStar.SharedKernel.Common.Primitives;
using MediatR;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;

namespace DoubleStar.Modules.Identity.Tests;

public sealed class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IPublisher> _publisher = new();

    private RegisterCustomerCommandHandler CreateHandler() => new(_identityService.Object, _publisher.Object);

    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_PublishesCustomerAccountRegisteredEvent()
    {
        var userId = Guid.NewGuid();
        _identityService
            .Setup(s => s.RegisterCustomerAsync("Ada", "Okafor", "ada@example.com", null, "Password123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(userId));

        var command = new RegisterCustomerCommand("Ada", "Okafor", "ada@example.com", null, "Password123!");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(userId);
        _publisher.Verify(p => p.Publish(
            It.Is<CustomerAccountRegisteredEvent>(e => e.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegistrationFails_DoesNotPublishEvent()
    {
        var error = new Error("Auth.EmailAlreadyExists", "taken", ErrorType.Conflict);
        _identityService
            .Setup(s => s.RegisterCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure(error));

        var command = new RegisterCustomerCommand("Ada", "Okafor", "ada@example.com", null, "Password123!");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _publisher.Verify(p => p.Publish(It.IsAny<CustomerAccountRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}