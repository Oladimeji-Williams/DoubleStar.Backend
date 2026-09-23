// SharedKernel/Contracts/Identity/CustomerAccountRegisteredEvent.cs
using MediatR;

namespace DoubleStar.SharedKernel.Contracts.Identity;

/// <summary>
/// Published by Identity right after a customer self-registers. The Customers
/// module handles it to create a Customer record whose Id equals UserId —
/// that's how the two modules line up without a cross-schema foreign key.
/// </summary>
public sealed record CustomerAccountRegisteredEvent(
    Guid UserId, string FirstName, string LastName, string? Email, string? Phone) : INotification;