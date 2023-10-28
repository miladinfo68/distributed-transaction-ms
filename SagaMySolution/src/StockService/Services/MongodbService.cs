using CommonService;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace StockService.Services;

public class MongodbService
{
    private readonly IMongoDatabase _db;

    public MongodbService(IOptions<MongodbConfigs> options)
    {
        var cfg = options.Value;
            
        var client = new MongoClient(cfg.ConnectionString);
        _db = client.GetDatabase(cfg.DbName);
    }
    public IMongoCollection<T> GetCollection<T>() => 
        _db.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}