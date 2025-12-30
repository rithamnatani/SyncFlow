using Api.Shared.Domain;
using Api.Shared.Infrastructure;
using Api.Features.Tickets.GetTickets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tickets.CreateTicket;

public class CreateTicketHandler
{
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public CreateTicketHandler(AppDbContext db, ISignalRService signalR)
    {
        _db = db;
        _signalR = signalR;
    }

    public record CreateTicketResult(TicketDto Ticket, SignalRMessageAction? SignalRMessage);

    public async Task<CreateTicketResult> HandleAsync(
        Guid projectId, 
        CreateTicketDto dto, 
        CancellationToken cancellationToken = default)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        // Get next ticket number for this project
        var maxTicketNumber = await _db.Tickets
            .Where(t => t.ProjectId == projectId)
            .MaxAsync(t => (int?)t.TicketNumber, cancellationToken) ?? 0;

        // Get max rank for ordering
        var maxRank = await _db.Tickets
            .Where(t => t.ProjectId == projectId && t.Status == TicketStatus.Todo)
            .MaxAsync(t => (double?)t.RankId, cancellationToken) ?? 0;

        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ProjectId = projectId,
            TicketNumber = maxTicketNumber + 1,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Priority = dto.Priority,
            Type = dto.Type,
            AssigneeId = dto.AssigneeId,
            Tags = dto.Tags,
            Estimate = dto.Estimate,
            Color = dto.Color,
            RankId = maxRank + 1000, // Leave gaps for reordering
            Status = TicketStatus.Todo
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        var ticketDto = new TicketDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Type,
            ticket.AssigneeId,
            ticket.Tags,
            ticket.Estimate,
            ticket.Color,
            ticket.RankId,
            ticket.CreatedAt,
            ticket.UpdatedAt
        );

        // Create SignalR message for real-time updates
        var signalRMessage = _signalR.SendToGroup(
            projectId.ToString(), 
            "TicketCreated", 
            ticketDto
        );

        return new CreateTicketResult(ticketDto, signalRMessage);
    }
}
