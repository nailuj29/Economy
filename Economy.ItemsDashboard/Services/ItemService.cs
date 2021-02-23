using System.Collections.Generic;
using Economy.ItemsDashboard.Models;
using MongoDB.Driver;

namespace Economy.ItemsDashboard.Services {
    public class ItemService {
        private readonly IMongoCollection<Item> _items;

        public ItemService(IDatabaseSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _items = database.GetCollection<Item>("Items");
        }

        public Item Create(Item item) {
            _items.InsertOne(item);
            return item;
        }

        public List<Item> Get() =>
            _items.Find(_ => true).ToList();

        public Item Get(string id) =>
            _items.Find(item => item.Id == id).SingleOrDefault();

        public Item Search(string query) =>
            _items.Find(item => item.Name.Contains(query)).SingleOrDefault();

        public void Update(Item item) =>
            _items.ReplaceOne(old => old.Id == item.Id, item);

        public void Delete(string id) =>
            _items.DeleteOne(item => item.Id == id);
    }
}