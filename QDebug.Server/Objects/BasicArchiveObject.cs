using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Server.Objects
{
    internal class BasicArchiveObject
    {
        // to write to mongodb
        [BsonElement("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("tag")]
        public string Tag { get; set; }

        [BsonElement("logs")]
        public Dictionary<string, string> Logs { get; set; } = new Dictionary<string, string>();

        public BasicArchiveObject(string tag, string addValue)
        {
            Tag = tag;
            Logs.Add(DateTime.Now.ToString(), addValue);
        }
    }
}
