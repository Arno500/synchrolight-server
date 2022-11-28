using NPSMLib;
using System;
using System.IO;

namespace LightServer.Types
{
    internal struct PlayerState
    {
        public MediaPlaybackState PlayState { get; set; }
        public TimeSpan Position { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public Stream AlbumImage { get; set; }
    }
}
