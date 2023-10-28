using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stock.API.Services
{
    public interface IProductService
    {
        public Task<List<Models.Product>> ProductListAsync();
        public Task<Models.Product> GetProductByIdAsync(int productId);
        public Task AddProductAsync(Models.Product product);
        public Task UpdateProductAsync(int productId, Models.Product product);
        public Task DeleteProductAsync(int productId);
    }


    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Models.Product> _collection;
        public ProductService( MongodbService mongodbService)
        {
            _collection = mongodbService.GetCollection<Models.Product>();
        }
        public async Task<List<Models.Product>> ProductListAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        public async Task<Models.Product> GetProductByIdAsync(int productId)
        {
            return await _collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync();
        }
        public async Task AddProductAsync(Models.Product product)
        {
            await _collection.InsertOneAsync(product);
        }
        public async Task UpdateProductAsync(int productId, Models.Product product)
        {
            await _collection.ReplaceOneAsync(x => x.ProductId == productId, product);
        }
        public async Task DeleteProductAsync(int productId)
        {
            await _collection.DeleteOneAsync(x => x.ProductId == productId);
        }
    }



}