using CommonService;
using CommonService.Entities;
using CommonService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Jobs;

public class NewOrdersListener : BackgroundService
{
    private readonly ICacheService _cache;
    private readonly IServiceScopeFactory _provider;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private readonly ILogger<NewOrdersListener> _logger;

    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly RedisConfigs _redisConfigs;

    public NewOrdersListener(
        ICacheService cache,
        IServiceScopeFactory provider,
        ILogger<NewOrdersListener> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        RedisConfigs redisConfigs)
    {
        _cache = cache;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _redisConfigs = redisConfigs;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);

        const string completeOrdersPattern = "order-*-complete-*";
        const string failOrdersPattern = "order-*-fail-*";
        const string paymentOrdersPattern = "order-*-payment-*";


        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                //complete orders
                await SendMessageToRedis(completeOrdersPattern, OrderStatus.Completed, stoppingToken);

                //fail orders
                await SendMessageToRedis(failOrdersPattern, OrderStatus.Failed, stoppingToken);

                //payment orders
                await SendMessageToRedis(paymentOrdersPattern, OrderStatus.Payment, stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
            }
        }
    }

    async ValueTask SendMessageToRedis(string redisCacheKeyPattern, OrderStatus orderStatus, CancellationToken token)
    {
        var cacheKeyOrderMessage = string.Empty;
        var __chacheKey = string.Empty;
        try
        {
            var redisOrders =
                _cache.GetKeysAndValuesByPattern2<OrderRequest>(redisCacheKeyPattern, token);

            await foreach (var (cacheKey, redisOrder) in redisOrders)
            {
                if (redisOrder.Tracked) continue;
                __chacheKey = cacheKey;

                cacheKeyOrderMessage = SetCacheKeyOrderMessage(orderStatus, redisOrder.OrderId);

                if (await _cache.IsExist(cacheKeyOrderMessage, token)) continue;

                await _cache.SetData<OrderResultMessage>(cacheKeyOrderMessage, new OrderResultMessage
                {
                    OrderId = redisOrder.OrderId,
                    OrderStatus = orderStatus,
                    Message = redisOrder.Description,
                    OrderItemResultMessages = redisOrder.OrderItems.Select(s =>
                        new OrderItemResultMessage
                        {
                            ProductId = s.ProductId,
                            Text = s.Description
                        }).ToList()
                }, token);

                var _ = await _cache.UpdateField<OrderRequest, bool>(cacheKey, "Tracked", true, token);
            }
        }
        catch (Exception e)
        {
            await _cache.RemoveData(cacheKeyOrderMessage, token);
            var _ = await _cache.UpdateField<OrderRequest, bool>(__chacheKey, "Tracked", false, token);
        }

        await ValueTask.CompletedTask;
    }

    string SetCacheKeyOrderMessage(OrderStatus orderStatus, int orderId)
    {
        return orderStatus switch
        {
            OrderStatus.Completed => $"order-{orderId}-completed-message",
            OrderStatus.Failed => $"order-{orderId}-failed-message",
            OrderStatus.Payment => $"order-{orderId}-payment-message",
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
        };
    }
}