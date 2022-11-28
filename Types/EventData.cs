using System.Collections.Generic;
using System.Linq;

namespace LightServer.Types
{
    public enum EventType
    {
        Global,
        PerZone
    }

    public struct EventDataDTO
    {
        public string Color { get; set; }
    }

    public struct EventData
    {
        public EventType EventType { get; set; }
        public Color Color { get; set; }
        public Dictionary<string, Color> ColorPerZone { get; set; }

        public EventData(LightEventDTO lightEventDTO, Dictionary<string, Dictionary<string, string>> colorMap)
        {
            if (lightEventDTO.Event.Color.StartsWith("#"))
            {
                Color = new Color(lightEventDTO.Event.Color);
                EventType = EventType.Global;
            }
            else
            {
                ColorPerZone = colorMap.ToDictionary(e => e.Key, e => new Color(e.Value.GetValueOrDefault(lightEventDTO.Event.Color, null)));
                EventType = EventType.PerZone;
            }
        }

    }
}
