using DoubleStar.Modules.Payments.Application.Abstractions;
using DoubleStar.Modules.Payments.Application.Commands.RefundPayment;
using DoubleStar.Modules.Payments.Domain.Entities;
using DoubleStar.Modules.Payments.Domain.Enums;
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Tests;

public sealed class RefundPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentTransactionRepository> _payments = new();

    private RefundPaymentCommandHandler CreateHandler() => new(_payments.Object);

    private static PaymentTransaction PaidTransaction(PaymentSourceType source = PaymentSourceType.Sale) =>
        PaymentTransaction.RecordManual(source, 1, 5_000, PaymentMethod.Cash);

    private void GivenStored(PaymentTransaction transaction) =>
        _payments
            .Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

    [Fact]
    public async Task Handle_RefundsPartOfAPayment_AndSavesIt()
    {
        var transaction = PaidTransaction();
        GivenStored(transaction);

        var result = await CreateHandler().Handle(
            new RefundPaymentCommand(transaction.Id, 2_000, "Returned one item"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        transaction.TotalRefundedKobo.Should().Be(2_000);
        _payments.Verify(r => r.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ForAnOnlineOrderPayment_RefundsLikeAnyOther()
    {
        var transaction = PaidTransaction(PaymentSourceType.Order);
        GivenStored(transaction);

        var result = await CreateHandler().Handle(
            new RefundPaymentCommand(transaction.Id, 5_000, "Order cancelled by shop"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task Handle_ForAnUnknownPayment_ReportsNotFound()
    {
        _payments
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTransaction?)null);

        var result = await CreateHandler().Handle(
            new RefundPaymentCommand(Guid.NewGuid(), 1_000, "Reason"), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Payments.NotFound");
        _payments.Verify(r => r.UpdateAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTheRefundIsMoreThanWasPaid_RefusesAndSavesNothing()
    {
        var transaction = PaidTransaction();
        GivenStored(transaction);

        var result = await CreateHandler().Handle(
            new RefundPaymentCommand(transaction.Id, 6_000, "Too much"), CancellationToken.None);

        result.Errors.Should().ContainSingle(e => e.Code == "Payments.InvalidState");
        transaction.TotalRefundedKobo.Should().Be(0);
        _payments.Verify(r => r.UpdateAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}