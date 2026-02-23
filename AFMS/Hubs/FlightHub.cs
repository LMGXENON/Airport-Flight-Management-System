using Microsoft.AspNetCore.SignalR;

namespace AFMS.Hubs;

public class FlightHub : Hub
{
    public async Task SubscribeToFlightUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "FlightUpdates");
    }

    public async Task UnsubscribeFromFlightUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FlightUpdates");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "FlightUpdates");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FlightUpdates");
        await base.OnDisconnectedAsync(exception);
    }
}
