// tests/Modules/DoubleStar.Modules.Identity.Tests/RegisterCustomerCommandHandlerTests.cs — full replacement
using Microsoft.AspNetCore.Hosting;
using MediatR;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;
using DoubleStar.Modules.Identity.Infrastructure.Turnstile;

namespace DoubleStar.Modules.Identity.Tests;

public sealed class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<ITurnstileVerifier> _turnstileVerifier = new();
    private readonly Mock<IWebHostEnvironment> _environment = new();
    private readonly Mock<IPublisher> _publisher = new();

    public RegisterCustomerCommandHandlerTests()
    {
        _environment.Setup(e => e.EnvironmentName).Returns("Testing");
    }

    private RegisterCustomerCommandHandler CreateHandler() =>
        new(_identityService.Object, _turnstileVerifier.Object, _environment.Object, _publisher.Object);

    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_PublishesCustomerAccountRegisteredEvent()
    {
        var userId = Guid.NewGuid();
        _identityService
            .Setup(s => s.RegisterCustomerAsync("ada@example.com", "Password123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(userId));

        var command = new RegisterCustomerCommand("ada@example.com", "Password123!", null);
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
            .Setup(s => s.RegisterCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure(error));

        var command = new RegisterCustomerCommand("ada@example.com", "Password123!", null);
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _publisher.Verify(p => p.Publish(It.IsAny<CustomerAccountRegisteredEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}