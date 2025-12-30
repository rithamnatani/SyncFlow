using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Tickets.CreateTicket;

public class CreateTicket
{
    private readonly ILogger<CreateTicket> _logger;
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public CreateTicket(ILogger<CreateTicket> logger, AppDbContext db, ISignalRService signalR)
    {
        _logger = logger;
        _db = db;
        _signalR = signalR;
    }

    [Function("CreateTicket")]
    [SignalROutput(HubName = "chat")]
    public async Task<CreateTicketResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectId}/tickets")] HttpRequestData req,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating ticket for project {ProjectId}", projectId);

        var dto = await req.ReadFromJsonAsync<CreateTicketDto>(cancellationToken);
        if (dto is null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid request body", cancellationToken);
            return new CreateTicketResponse { HttpResponse = badRequest };
        }

        try
        {
            var handler = new CreateTicketHandler(_db, _signalR);
            var result = await handler.HandleAsync(projectId, dto, cancellationToken);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(result.Ticket, cancellationToken);

            return new CreateTicketResponse
            {
                HttpResponse = response,
                SignalRMessages = result.SignalRMessage is not null ? [result.SignalRMessage] : []
            };
        }
        catch (ArgumentException ex)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(ex.Message, cancellationToken);
            return new CreateTicketResponse { HttpResponse = badRequest };
        }
    }
}

public class CreateTicketResponse
{
    [HttpResult]
    public required HttpResponseData HttpResponse { get; set; }

    [SignalROutput(HubName = "chat")]
    public SignalRMessageAction[] SignalRMessages { get; set; } = [];
}
