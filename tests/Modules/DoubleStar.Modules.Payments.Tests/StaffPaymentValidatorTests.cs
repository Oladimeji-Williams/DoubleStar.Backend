using DoubleStar.Modules.Payments.Application.Commands.InitializePaystackPayment;
using DoubleStar.Modules.Payments.Application.Commands.RecordManualPayment;
using DoubleStar.Modules.Payments.Domain.Enums;
using DoubleStar.SharedKernel.Contracts.Payments;

namespace DoubleStar.Modules.Payments.Tests;

public sealed class StaffPaymentValidatorTests
{
    [Fact]
    public void RecordManual_RejectsOnlineOrders()
    {
        var result = new RecordManualPaymentCommandValidator()
            .Validate(new RecordManualPaymentCommand(PaymentSourceType.Order, 1, 1_000, PaymentMethod.Cash));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(PaymentSourceType.Sale)]
    [InlineData(PaymentSourceType.RepairJob)]
    public void RecordManual_StillAcceptsSalesAndRepairs(PaymentSourceType source)
    {
        var result = new RecordManualPaymentCommandValidator()
            .Validate(new RecordManualPaymentCommand(source, 1, 1_000, PaymentMethod.Cash));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void InitializePaystack_RejectsOnlineOrders()
    {
        var result = new InitializePaystackPaymentCommandValidator().Validate(
            new InitializePaystackPaymentCommand(
                PaymentSourceType.Order, 1, 1_000, "ada@example.com", "https://shop.test/callback"));

        result.IsValid.Should().BeFalse();
    }
}