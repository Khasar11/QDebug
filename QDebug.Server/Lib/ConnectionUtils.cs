using QDebug.Server.Connections;
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
    }
}
