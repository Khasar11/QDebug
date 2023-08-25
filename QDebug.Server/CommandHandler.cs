using QDebug.Server.Commands;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Server.Events.Commands;
using S7.Net;

namespace QDebug.Server
{
    internal class CommandHandler
    {

        private Startup _application;
        public CommandHandler(Startup application)
        {
            _application = application;
        }
        public void EvaluateString(string input, ref bool stopEvaluating)
        {
            if (input == "stop") stopEvaluating = false;
            string[] args = input.Split(" ");
            string command = args[0];
            args = args.Skip(1).ToArray(); // remove first entry 
            switch (command)
            {
                case "plc":
                    new PLCCommand(_application, args);
                    break;
                case "opc":
                    new OPCCommand(_application, args);
                    break;
                default:
                    _application.Logger.Debug("Nonexistant command");
                    break;
            }
        }
    }
}
