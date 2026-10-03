// Identity/Api/Contracts/UpdateProfileRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record UpdateProfileRequest(string FirstName, string LastName, string? Phone);