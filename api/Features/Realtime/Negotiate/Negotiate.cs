using Api.Shared.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Api.Features.Realtime.Negotiate;

public class Negotiate
{
    private readonly ILogger<Negotiate> _logger;

    public Negotiate(ILogger<Negotiate> logger)
    {
        _logger = logger;
    }

    [Function("Negotiate")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "chat")] SignalRConnectionInfo connectionInfo)
    {
        _logger.LogInformation("SignalR negotiate request");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(connectionInfo);
        return response;
    }
}
