using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Contexts;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public OrdersController(ApplicationDbContext applicationDbContext,
            ISendEndpointProvider sendEndpointProvider)
        {
            _applicationDbContext = applicationDbContext;
            _sendEndpointProvider = sendEndpointProvider;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderVM model)
        {
            var order = new Models.Order()
            {
                BuyerId = model.BuyerId,
                OrderItems = model.OrderItems.Select(oi => new OrderItem
                {
                    Count = oi.Count,
                    Price = oi.Price,
                    ProductId = oi.ProductId,
                    OrderDate = DateTime.Now
                }).ToList(),
                OrderStatus = OrderStatus.Suspend,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderDate = DateTime.Now
            };

            await _applicationDbContext.AddAsync<Order.API.Models.Order>(order);

            await _applicationDbContext.SaveChangesAsync();

            var orderStartedEvent = new OrderStartedEvent()
            {
                BuyerId = model.BuyerId,
                OrderId = order.Id,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems.Select(oi => new Shared.OrderItemMessage
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Order_Orchestrator_Queue}"));
            await sendEndpoint.Send<OrderStartedEvent>(orderStartedEvent);
            return Ok(true);
        }
    }
}
