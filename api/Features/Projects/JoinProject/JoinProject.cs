using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Projects.JoinProject;

public class JoinProject
{
    private readonly ILogger<JoinProject> _logger;
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public JoinProject(ILogger<JoinProject> logger, AppDbContext db, ISignalRService signalR)
    {
        _logger = logger;
        _db = db;
        _signalR = signalR;
    }

    [Function("JoinProject")]
    [SignalROutput(HubName = "chat")]
    public async Task<JoinProjectResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectId}/join")] HttpRequestData req,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Join request for project {ProjectId}", projectId);

        // Get the current user
        var userService = new CurrentUserService(req);
        if (!userService.IsAuthenticated)
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Authentication required", cancellationToken);
            return new JoinProjectResponse { HttpResponse = unauthorized };
        }

        var handler = new JoinProjectHandler(_db, _signalR);
        var result = await handler.HandleAsync(projectId, userService.UserId!, cancellationToken);

        if (!result.Success)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(result.ErrorMessage ?? "Request failed", cancellationToken);
            return new JoinProjectResponse { HttpResponse = badRequest };
        }

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(new
        {
            result.Request!.Id,
            result.Request.ProjectId,
            result.Request.Status,
            result.Request.CreatedAt
        }, cancellationToken);

        return new JoinProjectResponse
        {
            HttpResponse = response,
            SignalRMessages = result.SignalRMessage is not null ? [result.SignalRMessage] : []
        };
    }
}

public class JoinProjectResponse
{
    [HttpResult]
    public required HttpResponseData HttpResponse { get; set; }

    [SignalROutput(HubName = "chat")]
    public SignalRMessageAction[] SignalRMessages { get; set; } = [];
}
