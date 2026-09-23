// Application/Mappings/PaymentMappings.cs
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Application.DTOs;
using DoubleStar.Modules.Payments.Domain.Entities;

namespace DoubleStar.Modules.Payments.Application.Mappings;

public static class PaymentMappings
{
    public static PaymentTransactionDto ToDto(this PaymentTransaction transaction) => new(
        transaction.Id, transaction.SourceType, transaction.SourceId, transaction.AmountKobo,
        transaction.TotalRefundedKobo, transaction.Method.ToString(), transaction.Status.ToString(),
        transaction.PaystackReference);

    public static PaymentSummaryDto ToSummaryDto(this PaymentTransaction transaction) => new(
        transaction.Id, transaction.SourceType, transaction.SourceId, transaction.AmountKobo,
        transaction.Method.ToString(), transaction.Status.ToString());
}