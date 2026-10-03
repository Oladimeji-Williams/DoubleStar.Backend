// Modules/Reviews/Persistence/ReviewsDbContext.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Reviews.Domain.Entities;

namespace DoubleStar.Modules.Reviews.Persistence;

public sealed class ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : DbContext(options)
{
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reviews");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewsDbContext).Assembly);
    }
}