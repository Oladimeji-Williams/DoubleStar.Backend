// Modules/Reviews/Application/DTOs/ReviewDto.cs
namespace DoubleStar.Modules.Reviews.Application.DTOs;

public sealed record ReviewDto(
    Guid Id, string ReviewerName, bool IsAnonymous, int Rating, string Comment,
    string Status, DateTime CreatedAtUtc, bool CanDelete);