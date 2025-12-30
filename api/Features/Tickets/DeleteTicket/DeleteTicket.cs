using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Tickets.DeleteTicket;

public class DeleteTicket
{
    private readonly ILogger<DeleteTicket> _logger;
    private readonly AppDbContext _db;
    private readonly ISignalRService _signalR;

    public DeleteTicket(ILogger<DeleteTicket> logger, AppDbContext db, ISignalRService signalR)
    {
        _logger = logger;
        _db = db;
        _signalR = signalR;
    }

    [Function("DeleteTicket")]
    [SignalROutput(HubName = "chat")]
    public async Task<DeleteTicketResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "tickets/{ticketId}")] HttpRequestData req,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting ticket {TicketId}", ticketId);

        var handler = new DeleteTicketHandler(_db, _signalR);
        var result = await handler.HandleAsync(ticketId, cancellationToken);

        if (!result.Success)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Ticket not found", cancellationToken);
            return new DeleteTicketResponse { HttpResponse = notFound };
        }

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return new DeleteTicketResponse
        {
            HttpResponse = response,
            SignalRMessages = result.SignalRMessage is not null ? [result.SignalRMessage] : []
        };
    }
}

public class DeleteTicketResponse
{
    [HttpResult]
    public required HttpResponseData HttpResponse { get; set; }

    [SignalROutput(HubName = "chat")]
    public SignalRMessageAction[] SignalRMessages { get; set; } = [];
}
