using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Economy.Models;
using MongoDB.Driver;

namespace Economy {
    public class DatabaseHelper {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Item> _items;
        public DatabaseHelper() {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_URI"));
            var database = client.GetDatabase("Economy");
            _users = database.GetCollection<User>("Users");
            _items = database.GetCollection<Item>("Items");
        }

        public async Task<List<User>> GetUsers() =>
            await _users.Find(Builders<User>.Filter.Empty).ToListAsync();

        public async Task<User> GetUser(string discordId) =>
            await _users.Find(user => user.DiscordId == discordId).FirstOrDefaultAsync();
        
        public async Task<User> GetUserByBsonId(string bsonId) =>
            await _users.Find(user => user.Id == bsonId).FirstOrDefaultAsync();
        
        public async Task<User> CreateUser(User user) {
            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task UpdateUser(string mongoId, User user) =>
            await _users.ReplaceOneAsync(oldUser => oldUser.Id == mongoId, user);
        
        public async Task DeleteUser(string mongoId) =>
            await _users.DeleteOneAsync(oldUser => oldUser.Id == mongoId);
        
        public async Task DeleteUser(User user) =>
            await _users.DeleteOneAsync(oldUser => oldUser.Id == user.Id);
        
        public async Task<Item> CreateItem(Item item) {
            await _items.InsertOneAsync(item);
            return item;
        }

        public async Task<List<Item>> GetItems() =>
            await _items.Find(Builders<Item>.Filter.Empty).ToListAsync();

        public async Task<Item> GetItem(string id) =>
            await _items.Find(item => item.Id == id).SingleOrDefaultAsync();

        public async Task<Item> SearchItems(string query) =>
            await _items.Find(item => item.Name.Contains(query)).SingleOrDefaultAsync();

        public async Task UpdateItem(Item item) =>
            await _items.ReplaceOneAsync(old => old.Id == item.Id, item);

        public async Task DeleteItem(string id) =>
            await _items.DeleteOneAsync(item => item.Id == id);
    }
}