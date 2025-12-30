using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tickets.MoveTicket;

public class MoveTicketHandler
{
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public MoveTicketHandler(AppDbContext db, ISignalRService signalR)
    {
        _db = db;
        _signalR = signalR;
    }

    public record MoveTicketResult(bool Success, string? ProjectId, SignalRMessageAction? SignalRMessage);

    public async Task<MoveTicketResult> HandleAsync(
        Guid ticketId,
        MoveTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return new MoveTicketResult(false, null, null);
        }

        var oldStatus = ticket.Status;
        ticket.Status = dto.NewStatus;
        ticket.RankId = dto.RankId;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        // Broadcast the move event
        var signalRMessage = _signalR.SendToGroup(
            ticket.ProjectId.ToString(),
            "TicketMoved",
            new
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus.ToString(),
                NewStatus = dto.NewStatus.ToString(),
                RankId = dto.RankId
            }
        );

        return new MoveTicketResult(true, ticket.ProjectId.ToString(), signalRMessage);
    }
}
