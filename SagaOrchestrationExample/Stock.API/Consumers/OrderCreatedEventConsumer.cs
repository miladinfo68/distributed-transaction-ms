using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Stock.API.Consumers
{
    public  class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
       private readonly MongodbService _mongodbService;
       private readonly ISendEndpointProvider _sendEndpointProvider;
       private readonly IPublishEndpoint _publishEndpoint;
       private readonly ILogger<OrderCreatedEventConsumer> _logger;

        public OrderCreatedEventConsumer(
            MongodbService mongodbService,
            ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndpoint, 
            ILogger<OrderCreatedEventConsumer> logger)
        {
            _mongodbService = mongodbService;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            var collection = _mongodbService.GetCollection<Models.Product>();

            foreach (var orderItem in context.Message.OrderItems)
                stockResult.Add(await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Order_Orchestrator_Queue}"));
            if (stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };
                await sendEndpoint.Send(stockReservedEvent);
                _logger.LogInformation($"yyy [Stock-Service] Payment has been done!");
            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stock not reserved..."
                };

                await sendEndpoint.Send(stockNotReservedEvent);
                _logger.LogInformation($"yyy [Stock-Service] Some order item not reserved!");
            }
        }
    }
}
