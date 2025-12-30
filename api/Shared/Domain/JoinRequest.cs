namespace Api.Shared.Domain;

/// <summary>
/// Pending project join requests/invitations.
/// </summary>
public class JoinRequest
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    /// <summary>
    /// Entra ID of the requester.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Status: "Pending" or "Declined"
    /// </summary>
    public string Status { get; set; } = "Pending";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    // Navigation
    public Project Project { get; init; } = null!;
}
