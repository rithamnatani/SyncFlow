namespace Api.Shared.Domain;

/// <summary>
/// Hardcoded Kanban columns based on the UI.
/// </summary>
public enum TicketStatus
{
    Todo = 0,
    InProgress = 1,
    Testing = 2,
    Done = 3
}

/// <summary>
/// Priority levels for tickets.
/// </summary>
public enum PriorityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
