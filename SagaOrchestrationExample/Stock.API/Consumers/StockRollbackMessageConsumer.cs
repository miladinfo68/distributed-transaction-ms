using System.Linq;
using MassTransit;
using Shared.Messages;
using Stock.API.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Stock.API.Consumers
{
    public  class StockRollbackMessageConsumer : IConsumer<StockRollBackMessage>
    {
        private readonly MongodbService _mongodbService;
        private readonly ILogger<StockRollbackMessageConsumer> _logger;

        public StockRollbackMessageConsumer(MongodbService mongodbService,
            ILogger<StockRollbackMessageConsumer> logger)
        {
            _mongodbService = mongodbService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var collection = _mongodbService.GetCollection<Models.Product>();
            if (!context.Message.OrderItems.Any()) return;
            
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await (await collection.FindAsync(s => s.ProductId == item.ProductId))
                    .FirstOrDefaultAsync();
                if (stock == null) continue;
                stock.Count += item.Count;
                await collection.FindOneAndReplaceAsync(s => s.ProductId == item.ProductId, stock);
            }
            _logger.LogInformation($"yyy [Payment-Service] Some Stocks has been rolled-back");
        }
    }
}