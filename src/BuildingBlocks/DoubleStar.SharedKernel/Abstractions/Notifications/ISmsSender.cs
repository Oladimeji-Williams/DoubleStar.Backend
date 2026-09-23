// Abstractions/Notifications/ISmsSender.cs
namespace DoubleStar.SharedKernel.Abstractions.Notifications;

public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken);
}