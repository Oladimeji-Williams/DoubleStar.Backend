// SeedData/SampleRepairSeedData.cs
using DoubleStar.Modules.Repairs.Domain.Entities;
using DoubleStar.Modules.Repairs.Persistence;
using DoubleStar.Modules.Customers.Domain.Entities;

namespace DoubleStar.DatabaseSeeder.SeedData;

public static class SampleRepairSeedData
{
    public static async Task SeedAsync(RepairsDbContext repairsDb, IReadOnlyList<Customer> customers, Guid technicianUserId)
    {
        var ticket = RepairTicket.Open(
            customers[1].Id, "iPhone 12, cracked screen", "IMEI-SAMPLE-0001", "Screen cracked after a drop, touch still works.");
        ticket.AssignTechnician(technicianUserId);
        ticket.RecordDiagnosis("Screen assembly needs replacement. No other damage found.", 25_000_00);
        ticket.ApproveQuote();
        // Deliberately left at InRepair — a live-looking ticket to build the frontend's "active repairs" view against.

        await repairsDb.RepairTickets.AddAsync(ticket);
        await repairsDb.SaveChangesAsync();
    }
}