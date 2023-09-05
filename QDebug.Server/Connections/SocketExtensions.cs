using QDebug.Shared.Logger;
using SocketIOSharp.Server.Client;

namespace QDebug.Server.Connections
{
    public static class SocketExtensions
    {
        public static void SendCache(this SocketIOSocket socket, Logger logger)
        {
            logger.Debug($"Sending MongoDB cache to {socket}");
        }
    }
}
