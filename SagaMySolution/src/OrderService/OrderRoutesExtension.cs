using System.Transactions;
using CommonService;
using CommonService.Entities;
using CommonService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.ViewModels;
using OrderItemVm = CommonService.Entities.OrderItemVm;

namespace OrderService;

public static class OrderRoutesExtension
{
    public static WebApplication MapOrderRoutes(this WebApplication app)
    {
        var orderGroup = app.MapGroup("/api/orders");

        orderGroup.MapGet("", async (OrderDbContext db) => await db.Orders.ToListAsync());

        orderGroup.MapGet("/{id:int}", async (OrderDbContext db, int id) =>
            await db.Orders.FirstOrDefaultAsync(x => x.Id == id));

        orderGroup.MapPost("", async (OrderDbContext db, ICacheService cache, OrderVm model) =>
        {
            var recentlyOrder = db.Orders
                .Where(a => a.BuyerId == model.BuyerId && a.OrderStatus == OrderStatus.Pending)
                .AsEnumerable()
                .FirstOrDefault(a => HasAnyRequestRecently(a.OrderDate));

            if (recentlyOrder is not null) return Results.Ok(true);

            var order = new Order()
            {
                BuyerId = model.BuyerId,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Count = oi.Count,
                    Price = oi.Price
                }).ToList()
            };

      
          
            
            
            await using var transaction =await db.Database.BeginTransactionAsync();
            try
            {
                await db.Orders.AddAsync(order);
                await db.SaveChangesAsync();
                
                var orderItemsVm = order.OrderItems.Select(s =>
                    new OrderItemVm(s.Id, s.ProductId, s.Count, s.Price)).ToList();

                var orderRequest = new OrderRequest(order.Id, orderItemsVm);
                OutboxOrder outBoxOrder = orderRequest;
                
                await db.OutboxOrders.AddAsync(outBoxOrder);
                await db.SaveChangesAsync();
                
                await transaction.CommitAsync();
                
                return Results.Ok(true);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return Results.Problem();
            }
           
            bool HasAnyRequestRecently(DateTime date)
            {
                var diffInSeconds = (DateTime.UtcNow - date).TotalSeconds;
                return diffInSeconds < 60;
            }
        });

        return app;
    }
}