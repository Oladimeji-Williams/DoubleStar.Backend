// Application/Queries/GetRepairTurnaroundQuery/GetRepairTurnaroundQuery.cs
namespace DoubleStar.Modules.Reporting.Application.Queries.GetRepairTurnaroundQuery;

public sealed record RepairTurnaroundDto(int CompletedCount, double AverageTurnaroundHours);

public sealed record GetRepairTurnaroundQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<Result<RepairTurnaroundDto>>;