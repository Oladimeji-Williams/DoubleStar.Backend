// Api/Contracts/CreateStaffRequest.cs
namespace DoubleStar.Modules.Identity.Api.Contracts;

public sealed record CreateStaffRequest(string FirstName, string LastName, string Email, string Password, string Role);