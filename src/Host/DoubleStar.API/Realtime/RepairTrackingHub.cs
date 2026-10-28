
using DoubleStar.Modules.Repairs.Application.Abstractions;
using DoubleStar.SharedKernel.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoubleStar.Api.Realtime;

[Authorize(Roles = "Customer")]
public sealed class RepairTrackingHub(
    IRepairTicketRepository repairTicketRepository,
    ICurrentUser currentUser) : Hub
{
    public static string GroupName(int ticketId) => $"repair-ticket:{ticketId}";

    public async Task SubscribeToTicket(int ticketId)
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new HubException("You must sign in to track a repair.");
        }

        var ticket = await repairTicketRepository.GetByIdAsync(
            ticketId,
            Context.ConnectionAborted);

        if (ticket is null)
        {
            throw new HubException("Repair ticket not found.");
        }

        if (ticket.CustomerId != userId)
        {
            throw new HubException(
                "You are not authorized to track this repair.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GroupName(ticketId),
            Context.ConnectionAborted);
    }

    public async Task UnsubscribeFromTicket(int ticketId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GroupName(ticketId),
            Context.ConnectionAborted);
    }
}
