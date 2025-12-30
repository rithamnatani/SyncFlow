using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Shared.Infrastructure;

/// <summary>
/// Interface for accessing the current user's identity.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID (Entra ID Subject).
    /// Returns null if not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Returns true if the request is authenticated (user or system).
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns true if this is a trusted system-to-system call (Agent impersonation).
    /// </summary>
    bool IsSystemCall { get; }
}

/// <summary>
/// Parses user identity from Easy Auth headers or Agent impersonation headers.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly string? _userId;
    private readonly bool _isSystemCall;

    public CurrentUserService(HttpRequestData request)
    {
        // Priority 1: Impersonation header (Agent/System call)
        // Only trust this if request has a valid Function Key (checked by Azure runtime)
        if (request.Headers.TryGetValues("X-SyncFlow-Acting-User-Id", out var actingUserHeader))
        {
            var actingUserId = actingUserHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(actingUserId))
            {
                _userId = actingUserId;
                _isSystemCall = true;
                return;
            }
        }

        // Priority 2: Easy Auth header (User login via Static Web Apps / App Service)
        if (request.Headers.TryGetValues("x-ms-client-principal", out var principalHeader))
        {
            var data = principalHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    var principal = JsonSerializer.Deserialize<ClientPrincipalData>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (principal?.UserId is not null)
                    {
                        _userId = principal.UserId;
                        _isSystemCall = false;
                        return;
                    }
                }
                catch
                {
                    // Invalid principal header, fall through to anonymous
                }
            }
        }

        // No authentication found
        _userId = null;
        _isSystemCall = false;
    }

    public string? UserId => _userId;
    public bool IsAuthenticated => _userId is not null;
    public bool IsSystemCall => _isSystemCall;

    /// <summary>
    /// Gets the user ID or throws UnauthorizedAccessException if not authenticated.
    /// </summary>
    public string GetRequiredUserId()
    {
        return _userId ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    private record ClientPrincipalData(
        string? IdentityProvider,
        string? UserId,
        string? UserDetails,
        IEnumerable<string>? UserRoles
    );
}
