namespace DoubleStar.Modules.Reviews.Application.Commands.RejectReviewCommand;
public sealed record RejectReviewCommand(Guid ReviewId) : IRequest<Result>;