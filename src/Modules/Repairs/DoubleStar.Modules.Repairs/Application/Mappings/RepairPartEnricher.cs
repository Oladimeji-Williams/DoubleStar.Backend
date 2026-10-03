// Modules/Repairs/Application/Mappings/RepairPartEnricher.cs
using DoubleStar.SharedKernel.Contracts.Catalog;
using DoubleStar.Modules.Repairs.Application.DTOs;

namespace DoubleStar.Modules.Repairs.Application.Mappings;

public static class RepairPartEnricher
{
    public static async Task<RepairTicketDto> WithProductNamesAsync(
        this RepairTicketDto ticket, IProductCatalog productCatalog, CancellationToken cancellationToken)
    {
        if (ticket.Parts.Count == 0) return ticket;
        var names = await ResolveNamesAsync(ticket.Parts.Select(p => p.ProductId), productCatalog, cancellationToken);
        return ticket with { Parts = ticket.Parts.Select(p => p with { ProductName = Name(names, p.ProductId) }).ToList() };
    }

    public static async Task<IReadOnlyList<RepairTicketDto>> WithProductNamesAsync(
        this IReadOnlyList<RepairTicketDto> tickets, IProductCatalog productCatalog, CancellationToken cancellationToken)
    {
        var names = await ResolveNamesAsync(tickets.SelectMany(t => t.Parts).Select(p => p.ProductId), productCatalog, cancellationToken);
        return tickets
            .Select(t => t with { Parts = t.Parts.Select(p => p with { ProductName = Name(names, p.ProductId) }).ToList() })
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