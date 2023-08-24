
using MongoDB.Bson;
using MongoDB.Driver;
using QDebug.Server.Connections.DB;
using QDebug.Server.Lib;
using QDebug.Shared.Logger;

namespace QDebug.Server.Connections
{
    public class MongoDBConnection : IDBConnection
    {
        #region IDBConnection implementation
        public EnumDBType Type { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Database { get;  set; }
        public Logger Logger { get; set; }
        #endregion IDBConnection implementation

        private string ConnectionString;
        private MongoClient? MongoClient { get; set; } = null;
        private IMongoDatabase? ConnectedDatabase = null;

        public MongoDBConnection(string database, string Ip, int port, Logger logger)
        {
            Database = database;
            IP = Ip;
            Port = port;
            Logger = logger;
            ConnectionString = $"mongodb://{IP}:{Port}";
        }

        public void ConnectSync()
        {
            try
            {
                Logger.Info($"MongoDB {Database} AT {ConnectionString} feedback: Connecting");
                MongoClient = new MongoClient(ConnectionString);
                ConnectedDatabase = MongoClient.GetDatabase(Database);
                bool isMongoLive = ConnectedDatabase.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
                if (isMongoLive) Logger.Info($"MongoDB {Database} AT {ConnectionString} feedback: Connection OK");
                else 
                    throw new Exception($"MongoDB {Database} AT {ConnectionString} feedback: Connection Failed, check your settings / MongoDB setup");
            }
            catch (Exception e) 
            { 
                // Insert mongodb failure here
                Logger.Error($"MongoDB Connection at {IP}:{Port} failed, data caching will not work {Environment.NewLine}{e}");
            }
        }

    }
}
