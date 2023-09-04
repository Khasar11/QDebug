
using QDebug.Server.Objects;
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


        // t == table or collection, object is object to write
        void Cache(string t, BasicObject value);

        object ReadCache(string t);
    }
}
