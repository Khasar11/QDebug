
using MongoDB.Bson;
using MongoDB.Driver;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Logger;
using S7.Net;

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
        public bool isConnected { get; set; } = false;
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
                if (isMongoLive)
                {
                    Logger.Info($"MongoDB {Database} AT {ConnectionString} feedback: Connection OK");  
                }
                else
                    throw new Exception($"MongoDB {Database} AT {ConnectionString} feedback: Connection Failed, check your settings / MongoDB setup");
                var timer = new Timer(_ => {
                    if (isMongoLive)
                    {
                        isConnected = true;
                    }
                    else
                    {
                        isConnected = false;
                        Logger.Warning($"MongoDB {Database} AT {ConnectionString} feedback: Disconnected");
                    }
                }, null, 0, 5000);
            }
            catch (Exception e) 
            { 
                // Insert mongodb failure here
                Logger.Error($"MongoDB Connection at {IP}:{Port} failed, data caching will not work {Environment.NewLine}{e}");
            }
        }

    }
}
