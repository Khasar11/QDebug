
using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Logger;

namespace QDebugServer
{
    class QDebugServer
    {
        static async Task Main(string[] args)
        {
            DateTime startupTime = DateTime.Now;

            ConsoleLogger ConsoleLogger = new ConsoleLogger();
            FileLogger FileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger logger = new Logger(ConsoleLogger);

            ServerConfiguration Config = new("./config.xml", logger);
            List<PLCConnection> PLCConnections = Config.DeserializePLCConnections();
            List<DBConnection> DBConnections = Config.DeserializeDBConnections();
            List<OPCUAConnection> OPCUAConnections = Config.DeserializeOPCConnections();
            foreach (DBConnection connection in DBConnections)
            {
                connection.IDBConnection.ConnectSync();
            }
            foreach (OPCUAConnection connection in OPCUAConnections)
            {
                await connection.ConnectAsync();
            }
            foreach (PLCConnection connection in PLCConnections)
            {
                await connection.ConnectAsync();
            }
        }
    }
}