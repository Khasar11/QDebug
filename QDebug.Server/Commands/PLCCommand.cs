﻿using QDebug.Server.Connections;
using S7.Net;

namespace QDebug.Server.Events.Commands
{
    internal class PLCCommand
    {

        private Startup _application;
        public PLCCommand(Startup application, string[] args)
        {
            _application = application;
            PLCConnection? connection = application.ConnectionUtils.FindPLCByIP(args[0]);
            if (connection == null)
            {
                application.Logger.Error("Plc connection is null");
                return;
            }
            if (connection.Plc == null)
            {
                application.Logger.Error("Connection.Plc is null");
                return;
            }
            if (!connection.Plc.IsConnected)
            {
                application.Logger.Error("Plc is not connected");
                return;
            }
            try
            {
                switch (args[1].ToLower())
                {
                    case "r":
                    case "read":
                        application.Logger.Info("Reading...");
                        Read(args); // subcommand
                        break;
                    case "w":
                    case "write":
                        application.Logger.Info("Writing...");
                        Write(args); // subcommand
                        break;
                    default:
                        throw new ArgumentException($"Wrongful input at index 0: {args[0]}, must be 'r'/'w'");
                }
            }
            catch (ArgumentException exception)
            {
                application.Logger.Error("Error in argument: " + exception.Message);
            }
            catch (IndexOutOfRangeException exception)
            {
                application.Logger.Error("Not enough arguments: " + exception.Message);
            }
        }

        private void Write(string[] args)
        {
            try
            {
                PLCConnection? connection = _application.PLCConnections.Find(x => x.IP == args[0]);
                if (connection == null || connection.Plc == null) throw new ArgumentNullException("Plc connection null");
                if (args[2] == null) throw new ArgumentNullException("Argument for address is empty");
                string type = args[2].Split(".")[1].Substring(0, 3);
                switch (type)
                {
                    case "DBX":
                        connection.Plc.Write(args[2], Boolean.Parse(args[3]));
                        break;
                    case "DBB":
                        connection.Plc.Write(args[2], Byte.Parse(args[3]));
                        break;
                    case "DBW":
                        connection.Plc.Write(args[2], Int16.Parse(args[3]));
                        break;
                    case "DBD":
                        connection.Plc.Write(args[2], Int32.Parse(args[3]));
                        break;
                    default:
                        throw new InvalidOperationException("Wrong type input");
                }
                _application.Logger.Info($"Wrote {args[3]} to {args[2]}");
            }
            catch (ArgumentNullException exception)
            {
                _application.Logger.Error("Plc not found " + exception.Message);
            }
            catch (PlcException exception)
            {
                _application.Logger.Error("Plc write error: " + exception.Message);
                if (exception.Message.Contains("Address out of range"))
                    _application.Logger.Debug("Check your block access settings, uncheck optimized or similar");
            }
            catch (InvalidAddressException exception)
            {
                _application.Logger.Error("Invalid address error: " + exception.Message);
            }
            catch (FormatException exception)
            {
                _application.Logger.Error("Wrong input format: " + exception.Message);
            }
            catch (Exception exception)
            {
                _application.Logger.Error("Generic error occured: " + exception.Message);
            }
        }

        private void Read(string[] args)
        {
            try
            {
                PLCConnection? connection = _application.ConnectionUtils.FindPLCByIP(args[0]);
                if (connection == null || connection.Plc == null) throw new ArgumentNullException("Plc connection null");
                if (args[2] == null) throw new ArgumentNullException("Argument for address is empty");
                var result = connection.Plc.Read(args[2]);
                _application.Logger.Info(result + "");
            }
            catch (ArgumentNullException exception)
            {
                _application.Logger.Error("Plc not found " + exception.Message);
            }
            catch (PlcException exception)
            {
                _application.Logger.Error("Plc read error: " + exception.Message);
                if (exception.Message.Contains("Address out of range"))
                    _application.Logger.Debug("Check your block access settings, uncheck optimized block access or similar, (Connection mechanisms PUT/GET?)");
            }
            catch (InvalidAddressException exception)
            {
                _application.Logger.Error("Invalid address error: " + exception.Message);
            }
            catch (FormatException exception)
            {
                _application.Logger.Error("Wrong input format: " + exception.Message);
            }
        }
    }
}
