namespace Api.Shared.Domain;

/// <summary>
/// Represents a Kanban board project.
/// </summary>
public class Project
{
    public Guid Id { get; init; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Entra ID Subject of the project owner.
    /// </summary>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Random 6-8 char code for public invite links.
    /// </summary>
    public string? InviteCode { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation Properties
    public List<Ticket> Tickets { get; init; } = [];
    public List<ProjectMember> Members { get; init; } = [];
    public List<JoinRequest> JoinRequests { get; init; } = [];
}
