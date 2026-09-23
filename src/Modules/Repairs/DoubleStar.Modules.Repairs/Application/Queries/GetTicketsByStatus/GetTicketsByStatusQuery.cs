// Application/Queries/GetTicketsByStatusQuery/GetTicketsByStatusQuery.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Queries.GetTicketsByStatusQuery;

public sealed record GetTicketsByStatusQuery(RepairStatus Status) : IRequest<Result<IReadOnlyList<RepairTicketDto>>>;