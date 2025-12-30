using Api.Shared.Domain;

namespace Api.Features.Tickets.MoveTicket;

/// <summary>
/// DTO for moving a ticket to a new status/position.
/// </summary>
public record MoveTicketDto
{
    public required TicketStatus NewStatus { get; init; }
    public double RankId { get; init; }
}
