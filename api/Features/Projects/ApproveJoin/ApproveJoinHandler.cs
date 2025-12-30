using Api.Shared.Domain;
using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Projects.ApproveJoin;

public class ApproveJoinHandler
{
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public ApproveJoinHandler(AppDbContext db, ISignalRService signalR)
    {
        _db = db;
        _signalR = signalR;
    }

    public record ApproveJoinResult(
        bool Success,
        string? ErrorMessage,
        SignalRMessageAction? SignalRMessage);

    public async Task<ApproveJoinResult> HandleAsync(
        Guid projectId,
        Guid requestId,
        string approverUserId,
        CancellationToken cancellationToken = default)
    {
        // Get the project and verify the approver is the owner
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            return new ApproveJoinResult(false, "Project not found", null);
        }

        if (project.OwnerId != approverUserId)
        {
            return new ApproveJoinResult(false, "Only the project owner can approve requests", null);
        }

        // Get the join request
        var joinRequest = await _db.JoinRequests
            .FirstOrDefaultAsync(jr => jr.Id == requestId && jr.ProjectId == projectId, cancellationToken);

        if (joinRequest is null)
        {
            return new ApproveJoinResult(false, "Join request not found", null);
        }

        if (joinRequest.Status != "Pending")
        {
            return new ApproveJoinResult(false, "Request is no longer pending", null);
        }

        // Create the membership
        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = joinRequest.UserId,
            Role = "Member"
        };

        // Remove the join request
        _db.JoinRequests.Remove(joinRequest);
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        // Notify the new member
        var signalRMessage = _signalR.SendToUser(
            joinRequest.UserId,
            "JoinRequestApproved",
            new
            {
                ProjectId = projectId,
                ProjectName = project.Name
            }
        );

        return new ApproveJoinResult(true, null, signalRMessage);
    }
}
