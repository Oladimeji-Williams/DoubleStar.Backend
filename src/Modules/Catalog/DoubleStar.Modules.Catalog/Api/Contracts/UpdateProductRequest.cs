// Api/Contracts/UpdateProductRequest.cs
namespace DoubleStar.Modules.Catalog.Api.Contracts;

public sealed record UpdateProductRequest(string Name, string? Description, int? CategoryId, int? BrandId);