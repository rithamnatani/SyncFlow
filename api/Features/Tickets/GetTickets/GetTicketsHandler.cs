using Api.Shared.Domain;
using Api.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Tickets.GetTickets;

public class GetTicketsHandler
{
    private readonly AppDbContext _db;

    public GetTicketsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TicketDto>> HandleAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var tickets = await _db.Tickets
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.RankId)
            .Select(t => new TicketDto(
                t.Id,
                t.TicketNumber,
                t.Title,
                t.Description,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Type,
                t.AssigneeId,
                t.Tags,
                t.Estimate,
                t.Color,
                t.RankId,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return tickets;
    }
}
