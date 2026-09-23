// Contracts/Catalog/CatalogErrors.cs
using DoubleStar.SharedKernel.Common.Primitives;

namespace DoubleStar.SharedKernel.Contracts.Catalog;

public static class CatalogErrors
{
    public static Error NotFound(int id) => new(
        "Catalog.NotFound", $"Product with ID '{id}' was not found.", ErrorType.NotFound);
}