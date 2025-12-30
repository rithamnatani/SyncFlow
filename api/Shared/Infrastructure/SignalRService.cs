using Microsoft.Azure.Functions.Worker;

namespace Api.Shared.Infrastructure;

/// <summary>
/// Interface for sending SignalR messages to clients.
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Sends a message to all clients connected to a specific group (e.g., project).
    /// </summary>
    SignalRMessageAction SendToGroup(string groupName, string target, object arguments);

    /// <summary>
    /// Sends a message to a specific user.
    /// </summary>
    SignalRMessageAction SendToUser(string userId, string target, object arguments);

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    SignalRMessageAction Broadcast(string target, object arguments);
}

/// <summary>
/// Creates SignalR message actions for Azure Functions output binding.
/// Hub name is "chat" per architecture spec.
/// </summary>
public class SignalRService : ISignalRService
{
    private const string HubName = "chat";

    public SignalRMessageAction SendToGroup(string groupName, string target, object arguments)
    {
        return new SignalRMessageAction(target)
        {
            GroupName = groupName,
            Arguments = [arguments]
        };
    }

    public SignalRMessageAction SendToUser(string userId, string target, object arguments)
    {
        return new SignalRMessageAction(target)
        {
            UserId = userId,
            Arguments = [arguments]
        };
    }

    public SignalRMessageAction Broadcast(string target, object arguments)
    {
        return new SignalRMessageAction(target)
        {
            Arguments = [arguments]
        };
    }
}
