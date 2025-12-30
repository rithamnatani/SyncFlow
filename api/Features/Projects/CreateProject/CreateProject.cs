using Api.Shared.Domain;
using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;

namespace Api.Features.Projects.CreateProject;

public class CreateProject
{
    private readonly ILogger<CreateProject> _logger;
    private readonly AppDbContext _db;

    public CreateProject(ILogger<CreateProject> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function("CreateProject")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new project");

        // Get the current user
        var userService = new CurrentUserService(req);
        if (!userService.IsAuthenticated)
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Authentication required", cancellationToken);
            return unauthorized;
        }

        var dto = await req.ReadFromJsonAsync<CreateProjectDto>(cancellationToken);
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Project name is required", cancellationToken);
            return badRequest;
        }

        // Generate a unique invite code
        var inviteCode = GenerateInviteCode();

        var project = new Project
        {
            Id = Guid.CreateVersion7(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            OwnerId = userService.UserId!,
            InviteCode = inviteCode
        };

        // Also add the owner as a member with Admin role
        var ownerMembership = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userService.UserId!,
            Role = "Admin"
        };

        _db.Projects.Add(project);
        _db.ProjectMembers.Add(ownerMembership);
        await _db.SaveChangesAsync(cancellationToken);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(new
        {
            project.Id,
            project.Name,
            project.Description,
            project.InviteCode,
            project.CreatedAt
        }, cancellationToken);

        return response;
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude ambiguous chars
        Span<byte> bytes = stackalloc byte[6];
        RandomNumberGenerator.Fill(bytes);
        
        return string.Create(6, bytes.ToArray(), (span, data) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = chars[data[i] % chars.Length];
            }
        });
    }
}
