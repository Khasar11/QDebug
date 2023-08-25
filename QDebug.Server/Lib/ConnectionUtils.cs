using MongoDB.Driver.Linq;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Server.Lib
{
    public class ConnectionUtils
    {
        Startup Application;
        public ConnectionUtils(Startup application)
        {
            Application = application;
        }

        public PLCConnection? FindPLCByIP(string IP)
        {
            try
            {
                PLCConnection? connection = Application.PLCConnections.Find(x => x.IP == IP);
                if (connection == null) throw new Exception("PLC not found in find attempt");
                return connection;
            } catch (Exception exception)
            {
                Application.Logger.Error("Erorr on FindPLCByIP attempt" + exception.Message);
            }
            return null;
        }
        public OPCUAConnection? FindOPCUAByIPPort(string IP, int port)
        {
            try
            {
                OPCUAConnection? connection = Application.OPCUAConnections.Find(x => x.Url.Contains(IP) && x.Url.Contains(port+""));
                if (connection == null) throw new Exception("OPCUa not found in find attempt");
                return connection;
            }
            catch (Exception exception)
            {
                Application.Logger.Error("Erorr on FindOPCUAByIPPort attempt" + exception.Message);
            }
            return null;
        }
        public DBConnection? FindDBByIPPort(string IP, int port)
        {
            try
            {
                DBConnection? connection = Application.DBConnections.Find(x => x.IDBConnection.IP == IP && x.IDBConnection.Port == port);
                if (connection == null) throw new Exception("DB not found in find attempt");
                return connection;
            }
            catch (Exception exception)
            {
                Application.Logger.Error("Erorr on FindDBByIPPort attempt" + exception.Message);
            }
            return null;
        }
    }
}
