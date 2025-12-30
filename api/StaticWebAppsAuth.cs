using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api
{
    public class ClientPrincipal
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        public IEnumerable<string>? UserRoles { get; set; }
    }

    public static class StaticWebAppsAuth
    {
        public static ClientPrincipal Parse(HttpRequestData req)
        {
            var principal = new ClientPrincipal();
            if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
            {
                var data = header.FirstOrDefault();
                if (!string.IsNullOrEmpty(data))
                {
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    var deserialized = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (deserialized != null)
                    {
                        principal = deserialized;
                    }
                }
            }
            return principal;
        }
    }
}
