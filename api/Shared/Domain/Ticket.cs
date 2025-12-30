namespace Api.Shared.Domain;

/// <summary>
/// Represents a task/ticket on the Kanban board.
/// </summary>
public class Ticket
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    /// <summary>
    /// User-friendly ticket number per project (1, 2, 3...).
    /// </summary>
    public int TicketNumber { get; init; }

    public required string Title { get; set; }

    /// <summary>
    /// Markdown-supported description.
    /// </summary>
    public string? Description { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Todo;

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    /// <summary>
    /// Type: "Story", "Bug", "Task", etc.
    /// </summary>
    public string Type { get; set; } = "Task";

    /// <summary>
    /// Entra ID of the assignee (nullable).
    /// </summary>
    public string? AssigneeId { get; set; }

    /// <summary>
    /// Tags stored as JSONB array.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Story point estimate.
    /// </summary>
    public double Estimate { get; set; }

    /// <summary>
    /// Card color for visual distinction.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Rank ID for ordering within a column.
    /// </summary>
    public double RankId { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Project Project { get; init; } = null!;
}
