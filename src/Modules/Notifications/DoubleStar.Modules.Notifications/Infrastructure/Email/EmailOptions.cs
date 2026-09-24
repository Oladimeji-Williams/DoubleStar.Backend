// Infrastructure/Email/EmailOptions.cs
namespace DoubleStar.Modules.Notifications.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public string FromAddress { get; init; } = null!;
    public string FromName { get; init; } = null!;
}