

namespace QDebug.Server.Connections.DB
{
    public class DBConnection
    {

        public readonly IDBConnection IDBConnection;

        public DBConnection(IDBConnection DBConnection)
        {
            IDBConnection = DBConnection;
        }
    }
}
