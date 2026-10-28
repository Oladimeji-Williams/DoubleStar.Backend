using MediatR;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.Modules.Identity.Application.Commands.RegisterCustomerCommand;
using DoubleStar.Modules.Identity.Application.Abstractions;


namespace DoubleStar.Modules.Identity.Tests;

public sealed class RegisterCustomerCommandHandlerTests
{
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IHumanVerifier> _humanVerifier = new();
    private readonly Mock<IPublisher> _publisher = new();

    public RegisterCustomerCommandHandlerTests()
    {
        _humanVerifier
            .Setup(v => v.VerifyAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private RegisterCustomerCommandHandler CreateHandler() =>
        new(_identityService.Object, _humanVerifier.Object, _publisher.Object);

// Add a new test:
    [Fact]
    public async Task Handle_WhenVerificationFails_ReturnsFailureAndSkipsRegistration()
    {
        _humanVerifier
            .Setup(v => v.VerifyAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateHandler().Handle(
            new RegisterCustomerCommand("ada@example.com", "Password123!", null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _identityService.Verify(
            s => s.RegisterCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_PublishesCustomerAccountRegisteredEvent()
    {
        var userId = Guid.NewGuid();

        var registrationOutcome = new RegistrationOutcome(
            userId,
            "ada@example.com",
            EmailSent: true,
            Message: "Your account was created successfully. Please check your email to confirm your account.");

        _identityService
            .Setup(s => s.RegisterCustomerAsync(
                "ada@example.com",
                "Password123!",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<RegistrationOutcome>.Success(registrationOutcome));

        var command = new RegisterCustomerCommand(
            "ada@example.com",
            "Password123!",
            null);

        var result = await CreateHandler()
            .Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(registrationOutcome);

        _publisher.Verify(
            p => p.Publish(
                It.Is<CustomerAccountRegisteredEvent>(
                    e =>
                        e.UserId == userId &&
                        e.Email == "ada@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegistrationFails_DoesNotPublishEvent()
    {
        var error = new Error(
            "Auth.EmailAlreadyExists",
            "taken",
            ErrorType.Conflict);

        _identityService
            .Setup(s => s.RegisterCustomerAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<RegistrationOutcome>.Failure(error));

        var command = new RegisterCustomerCommand(
            "ada@example.com",
            "Password123!",
            null);

        var result = await CreateHandler()
            .Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _publisher.Verify(
            p => p.Publish(
                It.IsAny<CustomerAccountRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}