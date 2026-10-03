// Modules/Reviews/Application/Commands/DeleteReviewCommand/DeleteReviewCommand.cs
namespace DoubleStar.Modules.Reviews.Application.Commands.DeleteReviewCommand;

public sealed record DeleteReviewCommand(Guid ReviewId) : IRequest<Result>;