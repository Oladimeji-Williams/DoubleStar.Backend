// Infrastructure/Paystack/PaystackOptions.cs
namespace DoubleStar.Modules.Payments.Infrastructure.Paystack;

public sealed class PaystackOptions
{
    public const string SectionName = "Paystack";
    public string SecretKey { get; init; } = null!;
}