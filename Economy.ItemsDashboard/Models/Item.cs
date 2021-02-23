using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Economy.ItemsDashboard.Models {
    public class Item {
        [BsonId] [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string EmoteId { get; set; }

        public long BuyPrice { get; set; }

        public long SellPrice { get; set; }
    }
}