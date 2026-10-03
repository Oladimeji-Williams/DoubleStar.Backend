// Modules/Reviews/Application/Commands/SubmitReviewCommand/SubmitReviewCommand.cs
using DoubleStar.Modules.Reviews.Application.DTOs;

namespace DoubleStar.Modules.Reviews.Application.Commands.SubmitReviewCommand;

public sealed record SubmitReviewCommand(int Rating, string Comment) : IRequest<Result<ReviewDto>>;