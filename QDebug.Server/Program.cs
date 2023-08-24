
using QDebug.Server;
using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Logger;

namespace QDebugServer
{
    class QDebugServer
    {
        private static bool reading = true;
        static void Main(string[] args)
        {
            DateTime startupTime = DateTime.Now;

            ConsoleLogger ConsoleLogger = new ConsoleLogger();
            FileLogger FileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger logger = new Logger(ConsoleLogger);

            ServerConfiguration Config = new("./config.xml", logger);
            List<PLCConnection> PLCConnections = Config.DeserializePLCConnections();
            List<DBConnection> DBConnections = Config.DeserializeDBConnections();
            List<OPCUAConnection> OPCUAConnections = Config.DeserializeOPCConnections();

            CommandHandler commandHandler = new CommandHandler(logger, ref PLCConnections, ref DBConnections, ref OPCUAConnections);

            foreach (DBConnection connection in DBConnections)
            {
                connection.IDBConnection.ConnectSync();
            }
            foreach (OPCUAConnection connection in OPCUAConnections)
            {
                connection.ConnectAsync();
            }
            foreach (PLCConnection connection in PLCConnections)
            {
                connection.ConnectAsync();
            }

            while (reading)
            {
                string? Read = Console.ReadLine();
                if (Read is not null)
                {
                    commandHandler.EvaluateString(Read, ref reading);
                }
            }
        }
    }
}