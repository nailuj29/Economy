using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Economy.Models;
using MongoDB.Driver;

namespace Economy {
    public class DatabaseHelper {
        private readonly IMongoCollection<User> users;
        public DatabaseHelper() {
            var client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_URI"));
            var database = client.GetDatabase("Economy");
            users = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetUsers() =>
            await users.Find(_ => true).ToListAsync();

        public async Task<User> GetUser(string discordId) =>
            await users.Find(user => user.DiscordId == discordId).FirstOrDefaultAsync();
        
        public async Task<User> GetUserByBsonId(string bsonId) =>
            await users.Find(user => user.Id == bsonId).FirstOrDefaultAsync();
        
        public async Task<User> Create(User user) {
            await users.InsertOneAsync(user);
            return user;
        }

        public async Task Update(string mongoId, User user) =>
            await users.ReplaceOneAsync(oldUser => oldUser.Id == mongoId, user);
        
        public async Task Delete(string mongoId) =>
            await users.DeleteOneAsync(oldUser => oldUser.Id == mongoId);
        
        public async Task Delete(User user) =>
            await users.DeleteOneAsync(oldUser => oldUser.Id == user.Id);
    }
}