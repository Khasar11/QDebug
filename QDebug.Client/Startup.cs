

using EngineIOSharp.Common.Enum;
using QDebug.Client.Configuration;
using QDebug.Shared.Logger;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
namespace QDebug.Client
{
    public class Startup
    {
        ConsoleLogger ConsoleLogger;
        FileLogger FileLogger;
        public Logger Logger;
        public static ClientConfiguration Config;
        CommandHandler CommandHandler;
        bool reading = true;
        DateTime startupTime = DateTime.Now;

        public SocketIOClient Client;

        private Startup _application;

        private string DestinationIP;
        private ushort DestinationPort;

        public Startup()
        {
            _application = this;

            Config = new("./client-config.xml", _application);

            ConsoleLogger = new ConsoleLogger(Config.GetConfigObject("/configuration/mode"));
            FileLogger = new FileLogger($"./logs/{startupTime.Year}_{startupTime.Month}_{startupTime.Day}_{startupTime.Hour}_{startupTime.Minute}_{startupTime.Second}.log");
            Logger = new Logger(ConsoleLogger);

            CommandHandler = new CommandHandler(_application);

            DestinationIP = Config.GetConfigObject("/configuration/socket/destination-ip");
            DestinationPort = ushort.Parse(Config.GetConfigObject("/configuration/socket/destination-port"));

            Client = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, DestinationIP, DestinationPort));
        }

        public void FireStartup()
        {

            Client.Connect();
            InitSocketClient();

            while (reading)
            {
                string? Read = Console.ReadLine();
                if (Read is not null)
                {
                    CommandHandler.EvaluateString(Read, ref reading);
                }
            }
        }

        public void InitSocketClient()
        {
            Logger.Info($"Initializing socket client @{DestinationIP}:{DestinationPort}");

            Client.On(SocketIOEvent.DISCONNECT, () =>
            {
                Logger.Info($"Disconnected from server at {DestinationIP}:{DestinationPort}");
            });
            Client.On(SocketIOEvent.CONNECTION, () =>
            {
                Logger.Info($"Connected to server at {DestinationIP}:{DestinationPort}");
            });
        }
    }
}
