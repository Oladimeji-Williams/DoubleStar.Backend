namespace DoubleStar.Modules.Reviews.Application.DTOs;

public sealed record ReviewDto(
    Guid Id, string ReviewerName, bool IsAnonymous, int Rating, string Comment,
    string Status, DateTime CreatedAtUtc, bool CanDelete,
    bool IsDeleted, DateTime? DeletedAtUtc, string? DeletedByName,
    DateTime? ApprovedAtUtc, string? ApprovedByName,
    DateTime? RejectedAtUtc, string? RejectedByName);