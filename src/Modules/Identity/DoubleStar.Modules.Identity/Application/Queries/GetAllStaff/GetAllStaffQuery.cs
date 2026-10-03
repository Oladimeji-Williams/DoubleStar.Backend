// Identity/Application/Queries/GetAllStaffQuery/GetAllStaffQuery.cs
using DoubleStar.SharedKernel.Contracts.Identity;

namespace DoubleStar.Modules.Identity.Application.Queries.GetAllStaffQuery;

public sealed record GetAllStaffQuery : IRequest<Result<IReadOnlyList<UserProfileDto>>>;