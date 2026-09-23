// Application/DTOs/RepairTicketDto.cs
using DoubleStar.SharedKernel.Contracts.Repairs;

namespace DoubleStar.Modules.Repairs.Application.DTOs;

public sealed record RepairPartDto(int Id, int ProductId, int Quantity, long UnitCostKobo, long TotalCostKobo);

public sealed record RepairTicketDto(
    int Id, Guid? CustomerId, string DeviceDescription, string? ImeiOrSerial, string FaultDescription,
    string? DiagnosisNotes, long? QuotedPriceKobo, Guid? TechnicianUserId, RepairStatus Status,
    IReadOnlyList<RepairPartDto> Parts);