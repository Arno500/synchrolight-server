using LightServer.Managers;
using LightServer.Types;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace LightServer.Server.Hubs
{
    public class BeaconHub : Hub
    {
        public async Task ReportStage(string id)
        {
            await Clients.All.SendAsync("ReceiveMessage", id);
        }

        public void SubscribeToChannel(string channelId)
        {
            BeaconsManager.Beacons.TryGetValue(Context.ConnectionId, out var beacon);
            beacon.SubscribedChannels.Add(channelId);
        }

        public override async Task OnConnectedAsync()
        {
            BeaconsManager.Beacons.Add(Context.ConnectionId, new Beacon(Context.ConnectionId));
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            BeaconsManager.Beacons.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
