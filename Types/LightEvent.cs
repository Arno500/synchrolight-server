using System;
using System.Collections.Generic;

namespace LightServer.Types
{
    public enum LightEventAnimation
    {
        Rainbow = 0
    }

    public struct LightEventDTO
    {
        public int Start { get; set; }
        public int End { get; set; }
        public EventDataDTO Event { get; set; }
    }

    public struct LightEvent
    {
        public LightEventAnimation? Animation { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public EventData Event { get; set; }

        public LightEvent(LightEventDTO le, Dictionary<string, Dictionary<string, string>> colorMap)
        {
            Start = DateTime.MinValue.AddMilliseconds(le.Start);
            End = DateTime.MinValue.AddMilliseconds(le.End);
            Event = new EventData(le, colorMap);
        }

    }
}
