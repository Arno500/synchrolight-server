using LightServer.Managers;
using LightServer.Server.Hubs;
using LightServer.Types;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace LightServer.Server
{
    internal class WebSocket
    {
        private static Timer repeatTimer = new Timer(1000);
        private static LightEvent? LastEvent = null;

        private static IHubContext<BeaconHub> hubContext { get; set; }

        public WebSocket(IHubContext<BeaconHub> hubContext)
        {
            WebSocket.hubContext = hubContext;
            repeatTimer.Elapsed += RepeatEvent;
        }

        internal async Task SendEvent(LightEvent lightEvent)
        {
            repeatTimer.Stop();
            LastEvent = lightEvent;
            await ProceedToSend(lightEvent);
            repeatTimer.Start();
            repeatTimer.AutoReset = true;
        }

        private void RepeatEvent(object sender, ElapsedEventArgs e)
        {
            if (LastEvent.HasValue && LastEvent.Value.Animation.HasValue)
            {
                _ = ProceedToSend(LastEvent.Value);
            }
        }

        private async Task ProceedToSend(LightEvent lightEvent)
        {
            foreach (var beaconElm in BeaconsManager.Beacons)
            {
                var beacon = beaconElm.Value;
                switch (lightEvent.Event.EventType)
                {
                    case EventType.Global:
                        await hubContext.Clients.Client(beacon.ConnectionId).SendAsync("LightEvent", new SendableEvent(lightEvent));
                        break;
                    case EventType.PerZone:
                        foreach (var channel in beacon.SubscribedChannels)
                        {
                            await hubContext.Clients.Client(beacon.ConnectionId).SendAsync("LightEvent", new SendableEvent(lightEvent, channel));
                        }
                        break;
                }
            }
        }
    }
}
