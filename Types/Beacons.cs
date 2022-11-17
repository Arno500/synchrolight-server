using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
