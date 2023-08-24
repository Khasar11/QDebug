
using QDebug.Server;
using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Logger;

namespace QDebugServer
{
    class QDebugServer
    {
        static void Main(string[] args)
        {
            Startup Startup = new Startup();
            Startup.FireStartup();
        }
    }
}