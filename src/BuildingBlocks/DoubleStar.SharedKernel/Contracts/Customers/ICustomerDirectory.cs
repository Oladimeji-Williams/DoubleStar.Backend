// Contracts/Customers/ICustomerDirectory.cs
namespace DoubleStar.SharedKernel.Contracts.Customers;

public interface ICustomerDirectory
{
    Task<CustomerSummaryDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Used by Sales for a walk-in with no account — finds-or-creates by phone.</summary>
    Task<CustomerSummaryDto> GetOrCreateWalkInAsync(
        string name, string? phone, CancellationToken cancellationToken = default);
}