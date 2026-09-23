// Domain/ValueObjects/Money.cs
namespace DoubleStar.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Always stored as the smallest currency unit (kobo for NGN) to avoid
/// floating-point rounding anywhere prices are compared or summed.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public long AmountKobo { get; }
    public string Currency { get; }

    private Money(long amountKobo, string currency)
    {
        AmountKobo = amountKobo;
        Currency = currency;
    }

    public static Money FromKobo(long amountKobo, string currency = "NGN")
    {
        if (amountKobo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amountKobo), "Amount cannot be negative.");
        }
        return new Money(amountKobo, currency);
    }

    public static Money Zero(string currency = "NGN") => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(AmountKobo + other.AmountKobo, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = AmountKobo - other.AmountKobo;
        if (result < 0)
        {
            throw new InvalidOperationException("Resulting amount cannot be negative.");
        }
        return new Money(result, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }
        return new Money(AmountKobo * quantity, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot operate on {Currency} and {other.Currency}.");
        }
    }

    public bool Equals(Money? other) =>
        other is not null && AmountKobo == other.AmountKobo && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(AmountKobo, Currency);
    public override string ToString() => $"{AmountKobo / 100m:N2} {Currency}";
}