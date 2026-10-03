// Identity/Api/Contracts/UpdateStaffRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record UpdateStaffRequest(string FirstName, string LastName, string Role);