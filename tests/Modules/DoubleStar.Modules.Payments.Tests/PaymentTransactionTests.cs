// PaymentTransactionTests.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Domain.Entities;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Tests;

public sealed class PaymentTransactionTests
{
    [Fact]
    public void RecordManual_WithPaystackMethod_Throws()
    {
        var act = () => PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 1000, PaymentMethod.Paystack);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IssueRefund_ForTheFullAmount_SetsStatusRefunded()
    {
        var transaction = PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 5000, PaymentMethod.Cash);
        transaction.IssueRefund(5000, "Customer returned item");
        transaction.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void IssueRefund_ForAPartialAmount_SetsStatusPartiallyRefunded()
    {
        var transaction = PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 5000, PaymentMethod.Cash);
        transaction.IssueRefund(2000, "Partial return");
        transaction.Status.Should().Be(PaymentStatus.PartiallyRefunded);
    }

    [Fact]
    public void IssueRefund_ExceedingTheOriginalAmount_Throws()
    {
        var transaction = PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 5000, PaymentMethod.Cash);
        var act = () => transaction.IssueRefund(6000, "too much");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IssueRefund_TwiceExceedingTheOriginalAmountInAggregate_Throws()
    {
        var transaction = PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 5000, PaymentMethod.Cash);
        transaction.IssueRefund(3000, "first partial");
        var act = () => transaction.IssueRefund(3000, "second partial");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkSuccessful_WhenNotPending_Throws()
    {
        var transaction = PaymentTransaction.RecordManual(PaymentSourceType.Sale, 1, 5000, PaymentMethod.Cash);
        var act = () => transaction.MarkSuccessful(); // already Successful from RecordManual
        act.Should().Throw<InvalidOperationException>();
    }
}