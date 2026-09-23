// Persistence/Repositories/RepairTicketRepository.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Persistence.Repositories;

public sealed class RepairTicketRepository(RepairsDbContext dbContext) : IRepairTicketRepository
{
    private IQueryable<RepairTicket> WithIncludes() =>
        dbContext.RepairTickets.Include(t => t.Parts).Include(t => t.StatusHistory);

    public Task<RepairTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        WithIncludes().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RepairTicket>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await WithIncludes().OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RepairTicket>> GetAllForCustomerAsync(
        Guid customerId, CancellationToken cancellationToken = default) =>
        await WithIncludes().Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RepairTicket>> GetByStatusAsync(
        RepairStatus status, CancellationToken cancellationToken = default) =>
        await WithIncludes().Where(t => t.Status == status)
            .OrderBy(t => t.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(RepairTicket ticket, CancellationToken cancellationToken = default)
    {
        await dbContext.RepairTickets.AddAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RepairTicket ticket, CancellationToken cancellationToken = default)
    {
        dbContext.RepairTickets.Update(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}