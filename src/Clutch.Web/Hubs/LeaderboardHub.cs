using Microsoft.AspNetCore.SignalR;

namespace Clutch.Web.Hubs;

public class LeaderboardHub : Hub
{
    public async Task JoinHackathonLeaderboard(int hackathonId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(hackathonId));
    }

    public async Task LeaveHackathonLeaderboard(int hackathonId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(hackathonId));
    }

    public static string GetGroupName(int hackathonId) => $"hackathon_{hackathonId}";
}
