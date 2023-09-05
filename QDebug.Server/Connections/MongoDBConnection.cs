
using MongoDB.Bson;
using MongoDB.Driver;
using QDebug.Server.Connections.DB;
using QDebug.Server.Objects;
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

        public void Cache(string t, BasicObject bObject)
        {

            IMongoCollection<BasicObject> mongoCollection = ConnectedDatabase.GetCollection<BasicObject>("opc"+t);

            var filter = Builders<BasicObject>.Filter.Eq(x => x.Tag, bObject.Tag);
            var opts = new UpdateOptions() { IsUpsert = true };

            var update = Builders<BasicObject>.Update;
            var updates = new List<UpdateDefinition<BasicObject>>();

            updates.Add(update.Set("tag", bObject.Tag));
            updates.Add(update.Set("value", bObject.Value));
            updates.Add(update.Set("lastWrite", DateTime.Now));

            BasicObject? oldObject = null;

            try
            {
                oldObject = mongoCollection.Find(filter).First();
            } catch (System.InvalidOperationException exception) { 
                Logger.Error($"Cant find old element in collection at {IP}:{Port}"); 
            }
            bool wroteOld = false;
            if (oldObject != null)
            {
                Dictionary<string, string> logs = oldObject.Logs != null ? oldObject.Logs : new Dictionary<string, string>();
                if (logs.Count >= 10)
                {
                    var dates = logs.Keys.ToList();
                    ArchiveAsync(bObject.Tag, DateTime.Now, logs[dates.OrderBy(date => date).First()], t); // insert oldest value into archive
                    logs.Remove(dates.OrderBy(date => date).First()); // remove oldest value
                }
                if (oldObject.Value != bObject.Value) logs.Add(DateTime.Now.ToString(), oldObject.Value);

                updates.Add(update.Set("logs", logs));
                wroteOld = true;
            }
            Logger.Debug($"Caching {bObject.Tag} : {bObject.Value} to collection {t}, logging old: {wroteOld}");

            mongoCollection.UpdateOne(filter, update.Combine(updates), opts);
        }

        private async Task ArchiveAsync(string tag, DateTime now, string v, string t)
        {
            IMongoCollection<BasicArchiveObject> archiveCollection = ConnectedDatabase.GetCollection<BasicArchiveObject>("opc"+t+"-archive");
            Logger.Debug($"Archiving {tag}, {now}, {v}");
                
            //archive instructions for BasicArchiveObject here
        }

        public object ReadCache(string t)
        {
            throw new NotImplementedException();
        }
    }
}
