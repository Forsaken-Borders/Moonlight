using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace Moonlight.Network
{
    public class MinecraftClient
    {
        public TcpClient TcpClient { get; private set; }

        public MinecraftClient(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            if (Program.Configuration.GetValue("online_mode", true))
            {
                // TODO: Setup encryption and username validation
            }

            if (Program.Configuration.GetValue("compression", true))
            {
                // TODO: Send compression packet
            }
        }
    }
}