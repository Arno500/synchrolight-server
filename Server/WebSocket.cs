using LightServer.Managers;
using LightServer.Server.Hubs;
using LightServer.Types;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Timers;

namespace LightServer.Server
{
    internal class WebSocket
    {
        private static Timer repeatTimer = new Timer(1000);
        private static LightEvent? LastEvent = null;

        private static IHubContext<BeaconHub> hubContext { get; set; }

        private static WebSocket instance { get; set; }

        public WebSocket(IHubContext<BeaconHub> hubContext)
        {
            WebSocket.hubContext = hubContext;
            repeatTimer.Elapsed += RepeatEvent;
            instance = this;
        }

        public static async Task SendEvent(LightEvent? lightEvent)
        {
            repeatTimer.Stop();
            LastEvent = lightEvent;
            await instance.ProceedToSend(lightEvent.Value);
            repeatTimer.Start();
            repeatTimer.AutoReset = true;
        }

        public static async Task SendEvent(SendableEvent? lightEvent)
        {
            repeatTimer.Stop();
            await hubContext.Clients.All.SendAsync("LightEvent", lightEvent);
        }

        public static async Task SendEvent()
        {
            repeatTimer.Stop();
            await hubContext.Clients.All.SendAsync("LightEvent", SendableEvent.EmptyEvent());
        }

        private void RepeatEvent(object sender, ElapsedEventArgs e)
        {
            if (LastEvent.HasValue && !LastEvent.Value.Animation.HasValue)
            {
                _ = ProceedToSend(LastEvent.Value);
            }
        }

        private async Task ProceedToSend(LightEvent lightEvent)
        {
            if (lightEvent.Event.EventType == EventType.Global)
            {
                await hubContext.Clients.All.SendAsync("LightEvent", new SendableEvent(lightEvent, null));
            }
            else
            {
                foreach (var beaconElm in BeaconsManager.Beacons)
                {
                    var beacon = beaconElm.Value;
                    switch (lightEvent.Event.EventType)
                    {
                        case EventType.PerZone:
                            foreach (var channel in beacon.SubscribedChannels)
                            {
                                if (lightEvent.Event.ColorPerZone.ContainsKey(channel))
                                {
                                    _ = hubContext.Clients.Client(beacon.ConnectionId).SendAsync("LightEvent", new SendableEvent(lightEvent, channel));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
