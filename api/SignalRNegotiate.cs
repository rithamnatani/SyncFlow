using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SyncFlow.Api
{
    public class SignalRNegotiate
    {
        private readonly ILogger<SignalRNegotiate> _logger;

        public SignalRNegotiate(ILogger<SignalRNegotiate> logger)
        {
            _logger = logger;
        }

        [Function("negotiate")]
        public static async Task<HttpResponseData> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "chat")] string connectionInfo)
        {
            var logger = req.FunctionContext.GetLogger("SignalRNegotiate");
            logger.LogInformation($"SignalR Connection Info: {connectionInfo}");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(connectionInfo);
            return response;
        }
    }
}
