using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Economy.Models {
    public class ItemRef {
        public MongoDBRef Ref { get; set; }
        public long Count { get; set; }
    }
}