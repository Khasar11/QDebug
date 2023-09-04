
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QDebug.Server.Objects
{
    public class BasicObject
    {

        // to write to mongodb
        [BsonElement("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("tag")]
        public string Tag { get; set; }

        [BsonElement("value")]
        public string Value{ get; set; }

        [BsonElement("logs")]
        public Dictionary<string, string> logs { get; set; } = new Dictionary<string, string>();

        [BsonElement("lastWrite")]
        public DateTime LastWrite { get; set; }

        public BasicObject(string tag, string addValue)
        {
            Tag = tag;
            Value = addValue;
        }

        public string Pretty()
        {
            return $"Tag: {Tag}, Value: {Value}";
        }
    }
}
