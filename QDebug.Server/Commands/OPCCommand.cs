
using Amazon.Runtime.Internal.Util;
using QDebug.Server.Connections;

namespace QDebug.Server.Commands
{
    internal class OPCCommand
    {
        private Startup _application;
        public OPCCommand(Startup application, string[] args)
        {
            _application = application;
            string prefix = "OPC";
            OPCUAConnection? connection = null;
            try
            {
                connection = application.ConnectionUtils.FindOPCUAByIPPort(args[0], Int16.Parse(args[1]));
            } catch (Exception exception)
            {
                application.Logger.Error($"{prefix} {args[0]}:{args[1]} error while finding {prefix}: {exception.Message}");
            }
            if (connection == null || connection.Client == null || !connection.Client.IsConnected)
            {
                application.Logger.Error($"{prefix} {args[0]}:{args[1]} connection error");
                return;
            }
            Console.WriteLine(args.Length);
            if (args.Length <= 2)
            {
                application.Logger.Warning("Missing arguments in command");
                return;
            }

            try
            {
                switch (args[2])
                {
                    case "sub":
                        application.Logger.Info("sub command");
                        Subscribe(args, connection);
                        break;
                    default:
                        throw new Exception($"Wrongful input at index 2 of subcommand {args[2]}");

                }
            } catch(Exception exception)
            {
                application.Logger.Error($"error on argument at position 2 ...{args[2]}...: {exception.Message}");
            }
        }
        private void Subscribe(string[] args, OPCUAConnection connection)
        {
            if (args[3] is null)
            {
                _application.Logger.Warning($"Lacking inputs at position 3 after {args[2]}");
                return;
            }
            _application.Logger.Debug($"Attempting to Subscribe to opcua at {connection.Url} - {args[3]}");

        }
    }
}
