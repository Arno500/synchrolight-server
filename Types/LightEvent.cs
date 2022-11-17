using System;

namespace LightServer.Types
{
    public enum LightEventAnimation
    {
        Rainbow = 0
    }

    public struct LightEvent
    {
        public LightEventAnimation? Animation { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public EventData Event { get; set; }

    }
}
