using System;
using System.Collections.Generic;

namespace LightServer.Types
{
    internal class SendableEvent
    {
        public Color Color { get; set; }
        public TimeSpan nextEventHint { get; set; }
        public SendableEvent(LightEvent lightEvent, string zone)
        {
            nextEventHint = lightEvent.End - lightEvent.Start;
            switch (lightEvent.Event.EventType)
            {
                case EventType.Global:
                    Color = lightEvent.Event.Color;
                    break;
                case EventType.PerZone:
                    Color = lightEvent.Event.ColorPerZone.GetValueOrDefault(zone);
                    break;
            }
        }

        public SendableEvent(LightEvent lightEvent)
        {
            nextEventHint = lightEvent.End - lightEvent.Start;
            Color = lightEvent.Event.Color;
        }

        public SendableEvent(ushort R, ushort G, ushort B)
        {
            Color = new Color((byte)R, (byte)G, (byte)B);
        }

        public SendableEvent() { }

        public static SendableEvent EmptyEvent()
        {
            var evt = new SendableEvent();
            evt.Color = new Color("#000000");
            return evt;
        }
    }
}
