using Microsoft.Extensions.Options;
using DoubleStar.Modules.Orders.Application.Abstractions;
using DoubleStar.Modules.Orders.Application.Commands.RetryOrderPayment;
using DoubleStar.Modules.Orders.Application.Options;
using DoubleStar.Modules.Orders.Domain.Entities;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using DoubleStar.SharedKernel.Common.Options;
using DoubleStar.SharedKernel.Common.Primitives;
using DoubleStar.SharedKernel.Contracts.Identity;
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Orders.Tests;

public sealed class RetryOrderPaymentCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<IPaymentInitiator> _initiator = new();
    private readonly Guid _ownerId = Guid.NewGuid();

    public RetryOrderPaymentCommandHandlerTests()
    {
        GivenProfileEmail("ada@example.com");
        _initiator
            .Setup(i => i.InitializeAsync(
                It.IsAny<PaymentSourceType>(), It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentInitiationDto>.Success(
                new PaymentInitiationDto(Guid.NewGuid(), "https://pay.test/abc", "REF-NEW")));
    }

    private RetryOrderPaymentCommandHandler CreateHandler(Guid userId) => new(
        _orders.Object,
        OrderTestSupport.Customer(userId).Object,
        _identity.Object,
        _initiator.Object,
        Options.Create(new OrdersOptions()),
        Options.Create(new FrontendOptions { BaseUrl = "http://localhost:4200", CustomerAppUrl = "http://localhost:4201/" }),
        new FixedTimeProvider(OrderTestSupport.Now));

    private void GivenStoredOrder(Order order) =>
        _orders.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

    private void GivenProfileEmail(string? email) =>
        _identity
            .Setup(i => i.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto(_ownerId, email, null, "Ada", "Obi", ["Customer"], true, null));

    private void VerifyPaymentWasNotStarted() =>
        _initiator.Verify(
            i => i.InitializeAsync(
                It.IsAny<PaymentSourceType>(), It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

    [Fact]
    public async Task Handle_ForAnUnpaidOrder_StartsPaymentAndReturnsTheCheckoutLink()
    {
        var order = OrderTestSupport.PendingOrder(_ownerId, OrderTestSupport.Now.AddMinutes(2));
        GivenStoredOrder(order);

        var result = await CreateHandler(_ownerId).Handle(new RetryOrderPaymentCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AuthorizationUrl.Should().Be("https://pay.test/abc");
        result.Value.PaymentReference.Should().Be("REF-NEW");
        order.PaymentReference.Should().Be("REF-NEW");
        _initiator.Verify(
            i => i.InitializeAsync(
                PaymentSourceType.Order,
                It.IsAny<int>(),
                order.TotalKobo,
                "ada@example.com",
                It.Is<string>(url => url.StartsWith("http://localhost:4201/checkout/callback?orderId=")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _orders.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ForAnotherCustomersOrder_ReportsNotFoundAndStartsNothing()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId));

        var result = await CreateHandler(Guid.NewGuid()).Handle(new RetryOrderPaymentCommand(1), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Orders.NotFound");
        VerifyPaymentWasNotStarted();
    }

    [Fact]
    public async Task Handle_ForAnOrderThatIsNoLongerAwaitingPayment_Refuses()
    {
        GivenStoredOrder(OrderTestSupport.PaidOrder(_ownerId));

        var result = await CreateHandler(_ownerId).Handle(new RetryOrderPaymentCommand(1), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Orders.NotPayable");
        VerifyPaymentWasNotStarted();
    }

    [Fact]
    public async Task Handle_AfterThePaymentWindowHasElapsed_Refuses()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId, OrderTestSupport.Now.AddMinutes(-1)));

        var result = await CreateHandler(_ownerId).Handle(new RetryOrderPaymentCommand(1), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Orders.PaymentWindowElapsed");
        VerifyPaymentWasNotStarted();
    }

    [Fact]
    public async Task Handle_WhenTheCustomerHasNoEmail_AsksForOne()
    {
        GivenStoredOrder(OrderTestSupport.PendingOrder(_ownerId));
        GivenProfileEmail(null);

        var result = await CreateHandler(_ownerId).Handle(new RetryOrderPaymentCommand(1), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Orders.EmailRequired");
        VerifyPaymentWasNotStarted();
    }
}