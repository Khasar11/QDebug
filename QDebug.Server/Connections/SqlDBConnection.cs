using QDebug.Server.Connections.DB;
using QDebug.Server.Objects;
using QDebug.Shared.Logger;

namespace QDebug.Server.Connections
{
    internal class SqlDBConnection : IDBConnection
    {
        public SqlDBConnection(int port, string IP, string database, Logger logger)
        {

        }

        public EnumDBType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string IP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Port { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Database { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Logger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool isConnected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Cache(string t, object value)
        {
            throw new NotImplementedException();
        }

        public void Cache(string t, BasicObject value)
        {
            throw new NotImplementedException();
        }

        public void ConnectSync()
        {
            throw new NotImplementedException();
        }

        public object ReadCache(string t)
        {
            throw new NotImplementedException();
        }
    }
}
