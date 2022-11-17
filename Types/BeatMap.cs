using System.Collections.Generic;
using System.Drawing;
namespace LightServer.Types
{
    public struct Metadata
    {
        public string Artist { get; set; }
        public string Title { get; set; }
    }

    internal class BeatMap
    {
        public Metadata Metadata { get; set; }
        public List<LightEvent> Events { get; set; }

        public Dictionary<string,Dictionary<string, Color>> ColorMap { get; set; }
    }
}
