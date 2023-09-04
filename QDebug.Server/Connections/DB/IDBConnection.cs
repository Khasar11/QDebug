
using QDebug.Shared.Logger;

namespace QDebug.Server.Connections.DB
{
    public interface IDBConnection
    {
        public EnumDBType Type { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public Logger Logger { get; set; }
        public bool isConnected { get; set; }
        void ConnectSync();
    }
}
