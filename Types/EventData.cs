using System.Collections.Generic;
using System.Drawing;

namespace LightServer.Types
{
    public enum EventType
    {
        Global,
        PerZone
    }

    public struct EventData 
    {
        public EventType EventType { get; set; }
        public Color Color {get; set;}
        public Dictionary<string, Color> ColorPerZone { get; set; }

    }
}
