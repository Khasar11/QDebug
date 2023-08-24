using QDebug.Shared.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
