// RepairTicketTests.cs
using DoubleStar.SharedKernel.Contracts.Repairs;
using DoubleStar.Modules.Repairs.Domain.Entities;

namespace DoubleStar.Modules.Repairs.Tests;

public sealed class RepairTicketTests
{
    [Fact]
    public void Open_SetsStatusToReceived()
    {
        var ticket = RepairTicket.Open(null, "iPhone 11", null, "Won't turn on");
        ticket.Status.Should().Be(RepairStatus.Received);
    }

    [Fact]
    public void RecordDiagnosis_FromReceived_EndsAtAwaitingApprovalWithTheQuoteStored()
    {
        var ticket = RepairTicket.Open(null, "iPhone 11", null, "Won't turn on");
        ticket.RecordDiagnosis("Dead battery", 15_000_00);

        ticket.Status.Should().Be(RepairStatus.AwaitingApproval);
        ticket.QuotedPriceKobo.Should().Be(15_000_00);
        ticket.StatusHistory.Should().HaveCount(2); // Received->Diagnosing, Diagnosing->AwaitingApproval
    }

    [Fact]
    public void ApproveQuote_WhenNotAwaitingApproval_Throws()
    {
        var ticket = RepairTicket.Open(null, "iPhone 11", null, "Won't turn on");
        var act = () => ticket.ApproveQuote();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddPart_WhenNotInRepair_Throws()
    {
        var ticket = RepairTicket.Open(null, "iPhone 11", null, "Won't turn on");
        var act = () => ticket.AddPart(productId: 1, quantity: 1, unitCostKobo: 5000_00);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Full_happy_path_reaches_Collected_with_five_history_entries()
    {
        var ticket = RepairTicket.Open(null, "iPhone 11", null, "Won't turn on");
        ticket.RecordDiagnosis("Dead battery", 15_000_00);
        ticket.ApproveQuote();
        ticket.AddPart(productId: 1, quantity: 1, unitCostKobo: 8_000_00);
        ticket.MarkReadyForCollection();
        ticket.CollectDevice();

        ticket.Status.Should().Be(RepairStatus.Collected);
        ticket.StatusHistory.Should().HaveCount(5);
        ticket.Parts.Should().ContainSingle();
    }

    [Theory]
    [InlineData(RepairStatus.Received)]
    [InlineData(RepairStatus.AwaitingApproval)]
    [InlineData(RepairStatus.InRepair)]
    [InlineData(RepairStatus.Ready)]
    public void Cancel_FromAnyNonTerminalStatus_Succeeds(RepairStatus status)
    {
        var ticket = BuildTicketAt(status);
        ticket.Cancel();
        ticket.Status.Should().Be(RepairStatus.Cancelled);
    }

    [Theory]
    [InlineData(RepairStatus.Collected)]
    [InlineData(RepairStatus.Cancelled)]
    public void Cancel_FromATerminalStatus_Throws(RepairStatus terminalStatus)
    {
        var ticket = BuildTicketAt(terminalStatus);
        var act = () => ticket.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    private static RepairTicket BuildTicketAt(RepairStatus status)
    {
        var ticket = RepairTicket.Open(null, "Test device", null, "Test fault");
        if (status == RepairStatus.Received) return ticket;

        ticket.RecordDiagnosis("Diagnosis", 10_000_00);
        if (status == RepairStatus.AwaitingApproval) return ticket;

        ticket.ApproveQuote();
        if (status == RepairStatus.InRepair) return ticket;

        ticket.MarkReadyForCollection();
        if (status == RepairStatus.Ready) return ticket;

        ticket.CollectDevice();
        return ticket; // Collected
    }
}