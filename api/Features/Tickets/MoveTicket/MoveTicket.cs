using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Tickets.MoveTicket;

public class MoveTicket
{
    private readonly ILogger<MoveTicket> _logger;
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public MoveTicket(ILogger<MoveTicket> logger, AppDbContext db, ISignalRService signalR)
    {
        _logger = logger;
        _db = db;
        _signalR = signalR;
    }

    [Function("MoveTicket")]
    [SignalROutput(HubName = "chat")]
    public async Task<MoveTicketResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "tickets/{ticketId}/move")] HttpRequestData req,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Moving ticket {TicketId}", ticketId);

        var dto = await req.ReadFromJsonAsync<MoveTicketDto>(cancellationToken);
        if (dto is null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid request body", cancellationToken);
            return new MoveTicketResponse { HttpResponse = badRequest };
        }

        var handler = new MoveTicketHandler(_db, _signalR);
        var result = await handler.HandleAsync(ticketId, dto, cancellationToken);

        if (!result.Success)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Ticket not found", cancellationToken);
            return new MoveTicketResponse { HttpResponse = notFound };
        }

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return new MoveTicketResponse
        {
            HttpResponse = response,
            SignalRMessages = result.SignalRMessage is not null ? [result.SignalRMessage] : []
        };
    }
}

public class MoveTicketResponse
{
    [HttpResult]
    public required HttpResponseData HttpResponse { get; set; }

    [SignalROutput(HubName = "chat")]
    public SignalRMessageAction[] SignalRMessages { get; set; } = [];
}
