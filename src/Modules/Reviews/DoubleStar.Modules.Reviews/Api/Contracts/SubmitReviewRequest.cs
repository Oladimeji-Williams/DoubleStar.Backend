// Modules/Reviews/Api/Contracts/SubmitReviewRequest.cs
namespace DoubleStar.Modules.Reviews.Api.Contracts;
public sealed record SubmitReviewRequest(int Rating, string Comment);