// Domain/Entities/PaymentTransaction.cs
using DoubleStar.SharedKernel.Domain;
using DoubleStar.SharedKernel.Contracts.Payments;
using DoubleStar.Modules.Payments.Domain.Enums;

namespace DoubleStar.Modules.Payments.Domain.Entities;

public sealed class PaymentTransaction : GuidEntity
{
    private readonly List<Refund> _refunds = [];

    public PaymentSourceType SourceType { get; private set; }
    public int SourceId { get; private set; }
    public long AmountKobo { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? PaystackReference { get; private set; }

    public long TotalRefundedKobo => _refunds.Sum(r => r.AmountKobo);
    public IReadOnlyList<Refund> Refunds => _refunds;

    private PaymentTransaction() { }

    /// <summary>A Paystack payment starts Pending until the callback/webhook verifies it.</summary>
    public static PaymentTransaction StartPaystack(
        PaymentSourceType sourceType, int sourceId, long amountKobo, string paystackReference) => new()
    {
        Id = Guid.NewGuid(),
        SourceType = sourceType,
        SourceId = sourceId,
        AmountKobo = amountKobo,
        Method = PaymentMethod.Paystack,
        Status = PaymentStatus.Pending,
        PaystackReference = paystackReference,
    };

    /// <summary>Cash/Transfer/Card recorded in person are Successful immediately — no verification step needed.</summary>
    public static PaymentTransaction RecordManual(
        PaymentSourceType sourceType, int sourceId, long amountKobo, PaymentMethod method)
    {
        if (method == PaymentMethod.Paystack)
        {
            throw new ArgumentException("Use StartPaystack for Paystack payments.", nameof(method));
        }

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            SourceType = sourceType,
            SourceId = sourceId,
            AmountKobo = amountKobo,
            Method = method,
            Status = PaymentStatus.Successful,
        };
    }

    public void MarkSuccessful()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark {Status} as Successful.");
        }
        Status = PaymentStatus.Successful;
    }

    public void MarkFailed()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark {Status} as Failed.");
        }
        Status = PaymentStatus.Failed;
    }

    public Refund IssueRefund(long amountKobo, string reason)
    {
        if (Status is not (PaymentStatus.Successful or PaymentStatus.PartiallyRefunded))
        {
            throw new InvalidOperationException($"Cannot refund a payment that is {Status}.");
        }
        if (amountKobo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountKobo));
        }
        if (TotalRefundedKobo + amountKobo > AmountKobo)
        {
            throw new InvalidOperationException("Refund amount exceeds the amount available to refund.");
        }

        var refund = Refund.Create(Id, amountKobo, reason);
        _refunds.Add(refund);

        Status = TotalRefundedKobo == AmountKobo ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;

        return refund;
    }
}