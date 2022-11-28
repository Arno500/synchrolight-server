using System.Collections.Generic;

namespace LightServer.Types
{
    internal class Beacon
    {
        public List<string> SubscribedChannels = new();

        public readonly string ConnectionId = "";

        public Beacon(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
