// Application/Queries/GetMyProfileQuery/GetMyProfileQuery.cs
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Queries.GetMyProfileQuery;

public sealed record GetMyProfileQuery : IRequest<Result<UserProfileDto>>;