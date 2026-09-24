// SharedKernel/Contracts/Repairs/RepairTicketSummaryDto.cs — replace the whole file
namespace DoubleStar.SharedKernel.Contracts.Repairs;

public sealed record RepairTicketSummaryDto(
    int Id, Guid? CustomerId, string DeviceDescription, RepairStatus Status, long? QuotedPriceKobo,
    DateTime CreatedAtUtc, DateTime? CollectedAtUtc);