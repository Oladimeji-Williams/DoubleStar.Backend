// Modules/Reviews/Application/Commands/ApproveReviewCommand/ApproveReviewCommand.cs
namespace DoubleStar.Modules.Reviews.Application.Commands.ApproveReviewCommand;
public sealed record ApproveReviewCommand(Guid ReviewId) : IRequest<Result>;