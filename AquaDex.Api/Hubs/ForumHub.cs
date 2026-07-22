using Microsoft.AspNetCore.SignalR;

namespace AquaDex.Api.Hubs;

public class ForumHub : Hub
{
    // Called by the client when they open a specific thread —
    // joins a "group" scoped to that thread so broadcasts can target just this thread's viewers
    public async Task JoinThread(int threadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread-{threadId}");
    }

    public async Task LeaveThread(int threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread-{threadId}");
    }
}