// SeedData/CustomerSeedData.cs
using Microsoft.EntityFrameworkCore;
using DoubleStar.Modules.Customers.Domain.Entities;
using DoubleStar.Modules.Customers.Persistence;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class CustomerSeedData
{
    public static async Task<IReadOnlyList<Customer>> SeedAsync(CustomersDbContext dbContext)
    {
        var customers = new[]
        {
            Customer.CreateWalkIn("Ngozi Chukwu", "+2348011112222", null),
            Customer.CreateWalkIn("Tunde Bakare", "+2348022223333", "tunde.bakare@example.com"),
            Customer.CreateWalkIn("Fatima Bello", "+2348033334444", null),
        };

        await dbContext.Customers.AddRangeAsync(customers);
        await dbContext.SaveChangesAsync();

        return customers;
    }
}