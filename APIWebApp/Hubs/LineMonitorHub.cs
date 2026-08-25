using Microsoft.AspNetCore.SignalR;
using APIWebApp.Hubs;

public class StatusNotifier
{
    private readonly IHubContext<LineMonitorHub> _hubContext;

    public StatusNotifier(IHubContext<LineMonitorHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushUpdateToClients(object dataPayload)
    {
        //"ReceiveLiveData" matches the listner string in cleint side (js code)
        await _hubContext.Clients.All.SendAsync("ReceiveLiveData", dataPayload);
    }
}

namespace APIWebApp.Hubs
{
    public class LineMonitorHub : Hub
    {
        // Optional: Method that web clients can invoke directly from JS
        public async Task BroadcastStatus(string statusMessage)
        {
            await Clients.All.SendAsync("ReceiveLiveData", statusMessage);
        }

        // Optional: Override connection events for logging/telemetry
        public override async Task OnConnectedAsync()
        {
            // Code runs when a new web client connects
            await base.OnConnectedAsync();
        }
    }
}