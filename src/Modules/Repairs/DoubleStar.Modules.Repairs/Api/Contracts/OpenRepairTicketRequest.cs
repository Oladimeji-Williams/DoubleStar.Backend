// Api/Contracts/OpenRepairTicketRequest.cs
namespace DoubleStar.Modules.Repairs.Api.Contracts;

public sealed record OpenRepairTicketRequest(
    Guid? CustomerId, string? WalkInName, string? WalkInPhone,
    string DeviceDescription, string? ImeiOrSerial, string FaultDescription);