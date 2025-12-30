using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tickets.DeleteTicket;

public class DeleteTicketHandler
{
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public DeleteTicketHandler(AppDbContext db, ISignalRService signalR)
    {
        _db = db;
        _signalR = signalR;
    }

    public record DeleteTicketResult(bool Success, string? ProjectId, SignalRMessageAction? SignalRMessage);

    public async Task<DeleteTicketResult> HandleAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return new DeleteTicketResult(false, null, null);
        }

        var projectId = ticket.ProjectId;
        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        // Broadcast the delete event
        var signalRMessage = _signalR.SendToGroup(
            projectId.ToString(),
            "TicketDeleted",
            new { TicketId = ticketId }
        );

        return new DeleteTicketResult(true, projectId.ToString(), signalRMessage);
    }
}
