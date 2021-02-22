using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Economy.Models {
    public class User {
        public User(string id) {
            DiscordId = id;
            Coins = 0;
        }
        
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string DiscordId { get; set; }
        
        public long Coins { get; set; }
    }
}