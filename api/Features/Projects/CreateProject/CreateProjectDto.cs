namespace Api.Features.Projects.CreateProject;

/// <summary>
/// DTO for creating a new project.
/// </summary>
public record CreateProjectDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
