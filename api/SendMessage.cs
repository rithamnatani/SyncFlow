using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SyncFlow.Api
{
    public class SendMessage
    {
        private readonly ILogger _logger;

        public SendMessage(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SendMessage>();
        }

        [Function("SendMessage")]
        public async Task<MultiOutput> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            using var reader = new StreamReader(req.Body);
            string requestBody = await reader.ReadToEndAsync();

            string message = string.IsNullOrEmpty(requestBody) ? "Test Message from API" : requestBody;

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Message sent to orders queue: {message}");

            return new MultiOutput
            {
                ServiceBusMessage = message,
                HttpResponse = response
            };
        }
    }

    public class MultiOutput
    {
        [ServiceBusOutput("orders", Connection = "ServiceBusConnection")]
        public string ServiceBusMessage { get; set; } = string.Empty;

        [HttpResult]
        public HttpResponseData HttpResponse { get; set; } = default!;
    }
}
