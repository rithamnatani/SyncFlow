using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Projects.ApproveJoin;

public class ApproveJoin
{
    private readonly ILogger<ApproveJoin> _logger;
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public ApproveJoin(ILogger<ApproveJoin> logger, AppDbContext db, ISignalRService signalR)
    {
        _logger = logger;
        _db = db;
        _signalR = signalR;
    }

    [Function("ApproveJoin")]
    [SignalROutput(HubName = "chat")]
    public async Task<ApproveJoinResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectId}/approve/{requestId}")] HttpRequestData req,
        Guid projectId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving join request {RequestId} for project {ProjectId}", requestId, projectId);

        // Get the current user
        var userService = new CurrentUserService(req);
        if (!userService.IsAuthenticated)
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Authentication required", cancellationToken);
            return new ApproveJoinResponse { HttpResponse = unauthorized };
        }

        var handler = new ApproveJoinHandler(_db, _signalR);
        var result = await handler.HandleAsync(projectId, requestId, userService.UserId!, cancellationToken);

        if (!result.Success)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(result.ErrorMessage ?? "Request failed", cancellationToken);
            return new ApproveJoinResponse { HttpResponse = badRequest };
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { Message = "Join request approved" }, cancellationToken);

        return new ApproveJoinResponse
        {
            HttpResponse = response,
            SignalRMessages = result.SignalRMessage is not null ? [result.SignalRMessage] : []
        };
    }
}

public class ApproveJoinResponse
{
    [HttpResult]
    public required HttpResponseData HttpResponse { get; set; }

    [SignalROutput(HubName = "chat")]
    public SignalRMessageAction[] SignalRMessages { get; set; } = [];
}
