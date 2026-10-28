namespace DoubleStar.Modules.Reviews.Application.Commands.DeleteReviewCommand;

public sealed record DeleteReviewCommand(Guid ReviewId) : IRequest<Result>;