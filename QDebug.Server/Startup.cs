using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Server.Lib;
using QDebug.Shared.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Server
{
    public class Startup
    {

        ConsoleLogger ConsoleLogger;
        FileLogger FileLogger;
        public Logger Logger;
        ServerConfiguration Config;
        public List<PLCConnection> PLCConnections;
        public List<DBConnection> DBConnections;
        public List<OPCUAConnection> OPCUAConnections;
        CommandHandler commandHandler;
        bool reading = true;
        DateTime startupTime = DateTime.Now;

        private Startup _application;
        public ConnectionUtils ConnectionUtils;

        public Startup()
        {
            _application = this;

            ConsoleLogger = new ConsoleLogger();
            FileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger = new Logger(ConsoleLogger);

            Config = new("./config.xml", _application);
            PLCConnections = Config.DeserializePLCConnections();
            DBConnections = Config.DeserializeDBConnections();
            OPCUAConnections = Config.DeserializeOPCConnections();

            ConnectionUtils = new ConnectionUtils(_application);
            commandHandler = new CommandHandler(_application);
        }

        public void FireStartup()
        {

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
