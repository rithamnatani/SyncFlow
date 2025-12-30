using Api.Shared.Domain;

namespace Api.Features.Tickets.CreateTicket;

/// <summary>
/// DTO for creating a new ticket.
/// </summary>
public record CreateTicketDto
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.Medium;
    public string Type { get; init; } = "Task";
    public string? AssigneeId { get; init; }
    public List<string> Tags { get; init; } = [];
    public double Estimate { get; init; }
    public string? Color { get; init; }
}
