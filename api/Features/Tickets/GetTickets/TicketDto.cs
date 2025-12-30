namespace Api.Features.Tickets.GetTickets;

/// <summary>
/// Response DTO for ticket list.
/// </summary>
public record TicketDto(
    Guid Id,
    int TicketNumber,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string Type,
    string? AssigneeId,
    List<string> Tags,
    double Estimate,
    string? Color,
    double RankId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
