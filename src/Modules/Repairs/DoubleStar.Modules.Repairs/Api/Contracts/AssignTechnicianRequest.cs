// Api/Contracts/AssignTechnicianRequest.cs
namespace DoubleStar.Modules.Repairs.Api.Contracts;

public sealed record AssignTechnicianRequest(Guid TechnicianUserId);