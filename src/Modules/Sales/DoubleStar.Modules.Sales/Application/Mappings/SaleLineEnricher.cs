// Modules/Sales/Application/Mappings/SaleLineEnricher.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Sales.Application.DTOs;

namespace DoubleStar.Modules.Sales.Application.Mappings;

public static class SaleLineEnricher
{
    public static async Task<SaleDto> WithProductNamesAsync(
        this SaleDto sale, IProductCatalog productCatalog, CancellationToken cancellationToken)
    {
        if (sale.Lines.Count == 0) return sale;

        var names = await ResolveNamesAsync(sale.Lines.Select(l => l.ProductId), productCatalog, cancellationToken);
        return sale with { Lines = sale.Lines.Select(l => l with { ProductName = Name(names, l.ProductId) }).ToList() };
    }

    public static async Task<IReadOnlyList<SaleDto>> WithProductNamesAsync(
        this IReadOnlyList<SaleDto> sales, IProductCatalog productCatalog, CancellationToken cancellationToken)
    {
        var productIds = sales.SelectMany(s => s.Lines).Select(l => l.ProductId);
        var names = await ResolveNamesAsync(productIds, productCatalog, cancellationToken);

        return sales
            .Select(s => s with { Lines = s.Lines.Select(l => l with { ProductName = Name(names, l.ProductId) }).ToList() })
            .ToList();
    }

    private static async Task<Dictionary<int, string>> ResolveNamesAsync(
        IEnumerable<int> productIds, IProductCatalog productCatalog, CancellationToken cancellationToken)
    {
        var distinctIds = productIds.Distinct().ToList();
        if (distinctIds.Count == 0) return new Dictionary<int, string>();

        var products = await productCatalog.GetByIdsAsync(distinctIds, cancellationToken);
        return products.ToDictionary(p => p.Id, p => p.Name);
    }

    private static string Name(IReadOnlyDictionary<int, string> names, int productId) =>
        names.GetValueOrDefault(productId, $"Product #{productId}");
}