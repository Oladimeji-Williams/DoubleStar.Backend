// BuildingBlocks.Infrastructure/Auditing/AuditingInterceptor.cs — replace the whole file
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using DoubleStar.SharedKernel.Domain;

namespace DoubleStar.BuildingBlocks.Infrastructure.Auditing;

public sealed class AuditingInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(IAuditable.ModifiedAt)).CurrentValue = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(IAuditable.ModifiedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }
}