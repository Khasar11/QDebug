using QDebug.Server.Configuration;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Server.Lib;
using QDebug.Shared.Logger;
using QDebug.Shared.Other;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace QDebug.Server
{
    public class Startup
    {

        ConsoleLogger ConsoleLogger;
        FileLogger FileLogger;
        public Logger Logger;
        public static ServerConfiguration Config;
        public List<PLCConnection> PLCConnections;
        public List<DBConnection> DBConnections;
        public List<OPCUAConnection> OPCUAConnections;
        CommandHandler CommandHandler;
        bool reading = true;
        DateTime startupTime = DateTime.Now;

        private Startup _application;
        public ConnectionUtils ConnectionUtils;

        public SocketIOServer? SServer;

        public Startup()
        {
            _application = this;

            Config = new("./server-config.xml", _application);

            ConsoleLogger = new ConsoleLogger(Config.GetConfigObject("/configuration/mode"));
            FileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger = new Logger(ConsoleLogger);


            PLCConnections = Config.DeserializePLCConnections();
            DBConnections = Config.DeserializeDBConnections();
            OPCUAConnections = Config.DeserializeOPCConnections();

            ConnectionUtils = new ConnectionUtils(_application);
            CommandHandler = new CommandHandler(_application);
            SServer = new SocketIOServer(new SocketIOServerOption(
                ushort.Parse(Config.GetConfigObject("/configuration/socketPort")
                    )
                ));
        }

        public void FireStartup()
        {

            SServer.Start();
            InitSocketServer();
            

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
                    CommandHandler.EvaluateString(Read, ref reading);
                }
            }
        }

        public void InitSocketServer()
        {
            Logger.Info($"Initializing socket server @{Utils.GetLocalIPAddress()}:{Config.GetConfigObject("/configuration/socketPort")}");

            SServer.OnConnection((SocketIOSocket socket) =>
            {
                Logger.Debug($"Socket connect: {socket}");

                socket.SendCache(Logger); // broadcast mongodb cache to user

                socket.On(SocketIOEvent.DISCONNECT, () =>
                {
                    Logger.Debug($"Socket disconnect: {socket}");
                });
            });
        }
    }
}
