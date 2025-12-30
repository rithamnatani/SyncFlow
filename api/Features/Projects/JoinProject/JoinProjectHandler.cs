using Api.Shared.Domain;
using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Projects.JoinProject;

public class JoinProjectHandler
{
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public JoinProjectHandler(AppDbContext db, ISignalRService signalR)
    {
        _db = db;
        _signalR = signalR;
    }

    public record JoinProjectResult(
        bool Success, 
        string? ErrorMessage, 
        JoinRequest? Request,
        SignalRMessageAction? SignalRMessage);

    public async Task<JoinProjectResult> HandleAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Check if project exists
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            return new JoinProjectResult(false, "Project not found", null, null);
        }

        // Check if user is already a member
        var existingMember = await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);

        if (existingMember)
        {
            return new JoinProjectResult(false, "User is already a member", null, null);
        }

        // Check if there's already a pending request
        var existingRequest = await _db.JoinRequests
            .AnyAsync(jr => jr.ProjectId == projectId && jr.UserId == userId && jr.Status == "Pending", cancellationToken);

        if (existingRequest)
        {
            return new JoinProjectResult(false, "Join request already pending", null, null);
        }

        // Create the join request
        var joinRequest = new JoinRequest
        {
            Id = Guid.CreateVersion7(),
            ProjectId = projectId,
            UserId = userId,
            Status = "Pending"
        };

        _db.JoinRequests.Add(joinRequest);
        await _db.SaveChangesAsync(cancellationToken);

        // Notify the project owner
        var signalRMessage = _signalR.SendToUser(
            project.OwnerId,
            "JoinRequestReceived",
            new
            {
                RequestId = joinRequest.Id,
                ProjectId = projectId,
                ProjectName = project.Name,
                UserId = userId
            }
        );

        return new JoinProjectResult(true, null, joinRequest, signalRMessage);
    }
}
