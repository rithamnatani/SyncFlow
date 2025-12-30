namespace Api.Shared.Domain;

/// <summary>
/// Join table for Users &lt;-&gt; Projects.
/// Composite PK: (ProjectId, UserId)
/// </summary>
public class ProjectMember
{
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Entra ID of the member.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Role: "Member" or "Admin"
    /// </summary>
    public string Role { get; set; } = "Member";

    public DateTimeOffset JoinedAt { get; init; } = DateTimeOffset.UtcNow;

    // Navigation
    public Project Project { get; init; } = null!;
}
