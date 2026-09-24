// Infrastructure/Sms/SmsOptions.cs
namespace DoubleStar.Modules.Notifications.Infrastructure.Sms;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";
    public string ApiKey { get; init; } = null!;
    public string SenderId { get; init; } = null!;
}