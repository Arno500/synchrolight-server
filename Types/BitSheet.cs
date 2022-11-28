using System.Collections.Generic;
using System.Linq;

namespace LightServer.Types
{
    public struct Metadata
    {
        public string Artist { get; set; }
        public string Title { get; set; }
    }

    public struct EventDto
    {
        public string Color { get; set; }
    }

    internal class BitSheet
    {
        public Metadata Metadata { get; set; }
        public List<LightEvent> Events { get; set; }

        public BitSheet(BitSheetDTO bs)
        {
            Metadata = bs.Metadata;
            Events = bs.BeatMap.Select(e => new LightEvent(e, bs.ColorMap)).ToList();
        }
    }

    internal class BitSheetDTO
    {
        public Metadata Metadata { get; set; }
        public Dictionary<string, Dictionary<string, string>> ColorMap { get; set; }
        public List<LightEventDTO> BeatMap { get; set; }
    }
}
