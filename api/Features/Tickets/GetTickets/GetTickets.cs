using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Tickets.GetTickets;

public class GetTickets
{
    private readonly ILogger<GetTickets> _logger;
    private readonly AppDbContext _db;

    public GetTickets(ILogger<GetTickets> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function("GetTickets")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectId}/tickets")] HttpRequestData req,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tickets for project {ProjectId}", projectId);

        var handler = new GetTicketsHandler(_db);
        var tickets = await handler.HandleAsync(projectId, cancellationToken);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(tickets, cancellationToken);
        return response;
    }
}
