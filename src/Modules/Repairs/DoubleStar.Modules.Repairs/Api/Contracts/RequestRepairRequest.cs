// Repairs/Api/Contracts/RequestRepairRequest.cs
namespace DoubleStar.Modules.Repairs.Api.Contracts;

public sealed record RequestRepairRequest(string DeviceDescription, string? ImeiOrSerial, string FaultDescription);