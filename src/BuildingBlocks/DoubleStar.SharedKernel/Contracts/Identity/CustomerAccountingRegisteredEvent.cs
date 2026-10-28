using MediatR;

namespace DoubleStar.SharedKernel.Contracts.Identity;

public sealed record CustomerAccountRegisteredEvent(Guid UserId, string Email) : INotification;