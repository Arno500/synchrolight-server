using System.Net;
using Haukcode.ArtNet.Sockets;
using Haukcode.ArtNet.Packets;
using Haukcode.ArtNet;
using System.Collections.Generic;
using LightServer.Managers;
using System.Threading;
using System;
using System.Security.Cryptography;

namespace LightServer.Server
{
    internal class ArtNetServer
    {
        private static readonly List<ArtNetSocket> sockets = new List<ArtNetSocket>();

        public ArtNetServer()
        {
            if (!SettingsManager.settings.ArtNetEnabled) return;
            lock (sockets) {
                var addresses = Helper.GetAddressesFromInterfaceType();
                foreach (var address in addresses)
                {
                    var socket = new ArtNetSocket
                    {
                        EnableBroadcast = true
                    };

                    socket.NewPacket += Socket_NewPacket;
                    socket.Open(address.Address, address.NetMask);
                    sockets.Add(socket);
                }
            }

        }

        protected void Socket_NewPacket(object sender, Haukcode.Sockets.NewPacketEventArgs<ArtNetPacket> e)
        {
            var socket = (ArtNetSocket)sender;
            switch (e.Packet.OpCode)
            {
                case ArtNetOpCodes.Poll:
                    var replyPacket = new ArtPollReplyPacket()
                    {
                        ShortName = "SynchroLight",
                        IpAddress = socket.LocalIP.GetAddressBytes(),
                        BindIpAddress = socket.LocalIP.GetAddressBytes(),
                        SwOut = new byte[] { (byte)SettingsManager.settings.DMXUniverse, (byte)0b00000000, (byte)0b00000000, (byte)0b00000000 },
                        PortCount = 1,
                        PortTypes = new byte[]{ (byte)0b10000000, (byte)0b00000000, (byte)0b00000000, (byte)0b00000000 },
                        FirmwareVersion = 1,
                    };
                    Thread.Sleep((int)((new Random()).NextDouble() * 1000));
                    socket.Send(replyPacket, new Haukcode.Sockets.RdmEndPoint(e.Source));
                    break;
                case ArtNetOpCodes.Dmx:
                    // TODO: Some data are comming, SEND TO THE LIGHTSTICKS!
                    var dmxData = (ArtNetDmxPacket)e.Packet;
                    if (dmxData.Universe != SettingsManager.settings.DMXUniverse) break;
                    break;
            }
        }

        public static void Dispose()
        {
            lock (sockets)
            {
                sockets.ForEach(artNetSocket =>
                {
                    artNetSocket.Close();
                    artNetSocket.Dispose();
                });
                sockets.Clear();
            }
        }
    }
}
