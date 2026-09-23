// Contracts/Repairs/RepairTicketSummaryDto.cs
namespace DoubleStar.SharedKernel.Contracts.Repairs;

public sealed record RepairTicketSummaryDto(
    int Id, Guid? CustomerId, string DeviceDescription, RepairStatus Status, long? QuotedPriceKobo);