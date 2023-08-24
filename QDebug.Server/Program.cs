
using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Configuration;
using QDebug.Shared.Logger;
using S7.Net;

namespace QDebugServer
{
    class QDebugServer
    {
        static async Task Main(string[] args)
        {
            DateTime startupTime = DateTime.Now;

            ConsoleLogger consoleLogger = new ConsoleLogger();
            FileLogger fileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger logger = new Logger(consoleLogger);

            ServerConfiguration Config = new("./config.xml", logger);
            List<PLCConnection> PLCConnections = Config.DeserializePLCConnections();
            List<DBConnection> DBConnections = Config.DeserializeDBConnections();
            List<OPCUAConnection> OPCUAConnections = Config.DeserializeOPCConnections();
            foreach (DBConnection connection in DBConnections)
            {
                connection.IDBConnection.ConnectSync();
            }
            foreach (PLCConnection connection in PLCConnections)
            {
                await connection.ConnectAsync();
            }
        }
    }
}