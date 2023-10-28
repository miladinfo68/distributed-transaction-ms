using CommonService.Entities;
using MongoDB.Driver;
using System.Threading.Tasks;
using StockService.Models;

namespace StockService.Services;

public interface IProductService
{
    public Task<List<Product>> ProductListAsync(IEnumerable<int>? productIds = null);
    public Task<Product?> GetProductByIdAsync(int productId);
    public Task AddProductAsync(Product product);
    public Task UpdateProductAsync(int productId, Product product);
    public Task DeleteProductAsync(int productId);
    public ValueTask<bool> BulkUpdateProductAsync(IEnumerable<OrderItemVm> products);
}

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _collection;

    public ProductService(MongodbService mongodbService)
    {
        _collection = mongodbService.GetCollection<Product>();
    }

    public async Task<List<Product>> ProductListAsync(IEnumerable<int>? productIds = null)
    {
        if (productIds is null) 
            return await _collection.Find(_ => true).ToListAsync();
        var filter = Builders<Product>.Filter.In(p => p.ProductId, productIds!);
        return await _collection.Find(filter).ToListAsync();
    }


    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _collection.Find(x => x.ProductId == productId).FirstOrDefaultAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        await _collection.InsertOneAsync(product);
    }

    public async Task UpdateProductAsync(int productId, Product product)
    {
        await _collection.ReplaceOneAsync(x => x.ProductId == productId, product);
    }

    public async Task DeleteProductAsync(int productId)
    {
        await _collection.DeleteOneAsync(x => x.ProductId == productId);
    }

    public async ValueTask<bool> BulkUpdateProductAsync(IEnumerable<OrderItemVm> products)
    {
        // List<OrderItemStatistics> orderItemStatistics = new();
        // products.ToList().ForEach(async p =>
        // {
        //     var orderItem = (await _collection.FindAsync(x => x.ProductId == p.ProductId))?.FirstOrDefault();
        //     if (orderItem is null)
        //     {
        //         orderItemStatistics.Add(
        //             new OrderItemStatistics(orderItem.ProductId, p.Count, 0, OrderItemStatus.NoExist,
        //                 "Order item not found"));
        //     }
        //     else
        //     {
        //         orderItemStatistics.Add(
        //             orderItem.Count >= p.Count
        //                 ? new OrderItemStatistics(orderItem.ProductId, p.Count, orderItem.Count, OrderItemStatus.Exist,
        //                     "")
        //                 : new OrderItemStatistics(orderItem.ProductId, p.Count, orderItem.Count,
        //                     OrderItemStatus.Insufficient, "Order item is inadequate")
        //         );
        //     }
        // });

        // if (orderItemStatistics.Any(a => a.Status != OrderItemStatus.Exist))
        // {
        //     //add data to queue to notify other micro
        // }
        // else
        // {
        //     // nothing
        // }


        await Task.CompletedTask;
        return true;
    }
}