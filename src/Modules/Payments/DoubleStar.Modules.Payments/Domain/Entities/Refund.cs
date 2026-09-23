// Domain/Entities/Refund.cs
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.Modules.Payments.Domain.Entities;

public sealed class Refund : Entity
{
    public Guid PaymentTransactionId { get; private set; }
    public long AmountKobo { get; private set; }
    public string Reason { get; private set; } = null!;

    private Refund() { }

    internal static Refund Create(Guid paymentTransactionId, long amountKobo, string reason) => new()
    {
        PaymentTransactionId = paymentTransactionId,
        AmountKobo = amountKobo,
        Reason = reason.Trim(),
    };
}