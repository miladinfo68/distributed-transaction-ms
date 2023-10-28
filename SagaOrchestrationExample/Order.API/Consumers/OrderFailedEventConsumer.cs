using MassTransit;
using Order.API.Models.Contexts;
using Order.API.Models.Enums;
using Shared.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Order.API.Consumers
{
    public  class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderFailedEventConsumer> _logger;

        public OrderFailedEventConsumer(ApplicationDbContext context, 
            ILogger<OrderFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            var order = await _context.FindAsync<Models.Order>(context.Message.OrderId);
            if (order is null) return;
            order.OrderStatus = OrderStatus.Fail;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"yyy [Order-Service] Order [{order.Id}] Failed! [{context.Message.Message}]");
        }
    }
}