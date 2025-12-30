using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class GetAuthInfo
    {
        private readonly ILogger _logger;

        public GetAuthInfo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetAuthInfo>();
        }

        [Function("GetAuthInfo")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var principal = StaticWebAppsAuth.Parse(req);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(principal);

            return response;
        }
    }
}
