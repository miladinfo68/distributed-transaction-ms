using MassTransit;
using Order.API.Models.Contexts;
using Order.API.Models.Enums;
using Shared.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Order.API.Consumers
{
    public  class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<OrderCompletedEventConsumer> _logger;

        public OrderCompletedEventConsumer(ApplicationDbContext applicationDbContext, 
            ILogger<OrderCompletedEventConsumer> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            var order = await _applicationDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order is null) return;
            order.OrderStatus = OrderStatus.Completed;
            await _applicationDbContext.SaveChangesAsync();
            
            _logger.LogInformation($"xxx [Order-Service] Order [{order.Id}] Completed!");
        }
    }
}