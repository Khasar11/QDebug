using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using S7.Net;

namespace QDebug.Server
{
    internal class CommandHandler
    {
        private QDebug.Shared.Logger.Logger Logger;
        private List<PLCConnection> PLCConnections;
        private List<DBConnection> DBConnections;
        private List<OPCUAConnection> OPCUAConnections;
        public CommandHandler(QDebug.Shared.Logger.Logger logger, ref List<PLCConnection> plcconnections, ref List<DBConnection> dbconnections, ref List<OPCUAConnection> opcuaconnections)
        {
            Logger = logger;
            PLCConnections = plcconnections;
            DBConnections = dbconnections;
            OPCUAConnections = opcuaconnections;
        }
        public void EvaluateString(string input, ref bool stopEvaluating)
        {
            if (input == "stop") stopEvaluating = false;
            string command = input.Split(" ")[0];
            if (command == "plc") PLC(input.Split(" "));
        }

        private void PLC(string[] args)
        {
            args = args.Skip(1).ToArray(); // remove first entry 
            PLCConnection? connection = PLCConnections.Find(x => x.IP == args[0]);
            if (connection == null)
            {
                Logger.Error("Plc connection is null");
                return;
            }
            if (connection.Plc == null)
            {
                Logger.Error("Connection.Plc is null");
                return;
            }
            if (!connection.Plc.IsConnected)
            {
                Logger.Error("Plc is not connected");
                return;
            }
            try
            {
                switch (args[1].ToLower())
                {
                    case "r":
                    case "read":
                        Logger.Info("Reading...");
                        HandleRead(args);
                        break;
                    case "w":
                    case "write":
                        Logger.Info("Writing...");
                        HandleWrite(args);
                        break;
                    default:
                        throw new ArgumentException($"Wrongful input at index 0: {args[0]}, must be r/w");
                }
            }
            catch (ArgumentException exception)
            {
                Logger.Error("Error in argument: " + exception.Message);
            }
            catch (IndexOutOfRangeException exception)
            {
                Logger.Error("Not enough arguments: " + exception.Message);
            }
        }
        private void HandleRead(string[] args)
        {
            try
            {
                PLCConnection? connection = PLCConnections.Find(x => x.IP == args[0]);
                if (connection == null || connection.Plc == null) throw new ArgumentNullException("Plc connection null");
                if (args[2] == null) throw new ArgumentNullException("Argument for address is empty");
                var result = connection.Plc.Read(args[2]);
                Logger.Info(result+"");
            } catch (ArgumentNullException exception) 
            { 
                Logger.Error("Plc not found " + exception.Message); 
            } catch (PlcException exception)
            {
                Logger.Error("Plc read error: " + exception.Message);
            } catch (InvalidAddressException exception)
            {
                Logger.Error("Invalid address error: " + exception.Message);
            } catch (FormatException exception)
            {
                Logger.Error("Wrong input format: " + exception.Message);
            }
        }
        private void HandleWrite(string[] args)
        {
            try
            {
                PLCConnection? connection = PLCConnections.Find(x => x.IP == args[0]);
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
                Logger.Info($"Wrote {args[3]} to {args[2]}");
            }
            catch (ArgumentNullException exception)
            {
                Logger.Error("Plc not found " + exception.Message);
            }
            catch (PlcException exception)
            {
                Logger.Error("Plc read error: " + exception.Message);
            }
            catch (InvalidAddressException exception)
            {
                Logger.Error("Invalid address error: " + exception.Message);
            }
            catch (FormatException exception)
            {
                Logger.Error("Wrong input format: " + exception.Message);
            } 
            catch (Exception exception)
            {
                Logger.Error("Generic error occured: " + exception.Message);
            }
        }
    }
}
