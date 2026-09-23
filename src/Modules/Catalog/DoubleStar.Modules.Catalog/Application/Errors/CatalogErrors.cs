// Application/Errors/CatalogErrors.cs
namespace DoubleStar.Modules.Catalog.Application.Errors;

public static class CatalogErrors
{
    public static Error NotFound(int id) => new(
        "Catalog.NotFound", $"Product with ID '{id}' was not found.", ErrorType.NotFound);

    public static Error SkuAlreadyExists(string sku) => new(
        "Catalog.SkuAlreadyExists", $"A product with SKU '{sku}' already exists.", ErrorType.Conflict);
}