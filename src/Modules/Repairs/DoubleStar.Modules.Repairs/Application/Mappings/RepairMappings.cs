// Application/Mappings/RepairMappings.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.DTOs;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Application.Mappings;

public static class RepairMappings
{
    public static RepairPartDto ToDto(this RepairPart part) =>
        new(part.Id, part.ProductId, part.Quantity, part.UnitCostKobo, part.TotalCostKobo);

    public static RepairTicketDto ToDto(this RepairTicket ticket) => new(
        ticket.Id, ticket.CustomerId, ticket.DeviceDescription, ticket.ImeiOrSerial, ticket.FaultDescription,
        ticket.DiagnosisNotes, ticket.QuotedPriceKobo, ticket.TechnicianUserId, ticket.Status,
        ticket.Parts.Select(p => p.ToDto()).ToList());

    public static RepairTicketSummaryDto ToSummaryDto(this RepairTicket ticket) => new(
        ticket.Id, ticket.CustomerId, ticket.DeviceDescription, ticket.Status, ticket.QuotedPriceKobo,
        ticket.CreatedAt,
        ticket.StatusHistory.FirstOrDefault(h => h.ToStatus == RepairStatus.Collected)?.CreatedAt);
}