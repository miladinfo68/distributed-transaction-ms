using System.Text.Json;
using CommonService;
using CommonService.Entities;
using CommonService.Services;
using Microsoft.Extensions.Options;
using StockService.Services;

namespace StockService.Jobs;

public class ProductCheckerListener : BackgroundService
{
    private readonly ICacheService _cache;
    private readonly IProductService _productService;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private readonly ILogger<ProductCheckerListener> _logger;
    private readonly RedisConfigs _redisConfigs;
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public ProductCheckerListener(
        ICacheService cache,
        IProductService productService,
        IOptions<RedisConfigs> redisConfigs,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ProductCheckerListener> logger)
    {
        _cache = cache;
        _productService = productService;
        _redisConfigs = redisConfigs.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);
        OrderRequest? orderRequest = default;
        const string pendingOrdersPattern = "order-*-pending-*";
        string? failCacheKey = default;

        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                var redisPendingOrders = _cache.GetKeysAndValuesByPattern2<OrderRequest>(pendingOrdersPattern, stoppingToken);
                await foreach (var (redisKey, pendingOrder) in redisPendingOrders)
                {
                    orderRequest = pendingOrder;
                    
                    if (string.IsNullOrEmpty(redisKey) || !orderRequest.OrderItems.Any()) continue;

                    orderRequest = new OrderRequest
                    {
                        OrderId = pendingOrder.OrderId,
                        Description = pendingOrder.Description ?? "",
                        OrderDate = pendingOrder.OrderDate,
                    };

                    foreach (var vm in orderRequest.OrderItems)
                    {
                        var dbProduct = await _productService.GetProductByIdAsync(vm.ProductId);
                        if (dbProduct is null)
                        {
                            orderRequest.OrderItems.Add(new OrderItemVm()
                            {
                                Id = vm.Id,
                                ProductId = vm.ProductId,
                                Price = vm.Price,
                                Count = vm.Count,
                                OrderItemStatus = OrderItemStatus.NoExist,
                                Description = "this product is not exist in database!"
                            });
                        }
                        else
                        {
                            if (dbProduct.Count >= vm.Count) continue;
                            orderRequest.OrderItems.Add(new OrderItemVm()
                            {
                                Id = vm.Id,
                                ProductId = vm.ProductId,
                                Price = vm.Price,
                                Count = vm.Count,
                                OrderItemStatus = OrderItemStatus.Insufficient,
                                Description =$"insufficient product in database, you can only choose {Math.Abs(dbProduct.Count - vm.Count)} numbers of that!"
                            });
                        }
                    }

                    if (!orderRequest.OrderItems.Any()) continue;

                    failCacheKey = string.Format(_redisConfigs.CacheKeyPendingFormat, orderRequest!.OrderId, orderRequest.OrderDate);
                    failCacheKey = string.Join("-", failCacheKey.Split('-')[0..3]) + "-*";

                    if (await _cache.IsExistAnyKey(failCacheKey, stoppingToken)) continue;
                    
                    await _cache.SetData<OrderRequest>(failCacheKey, orderRequest, stoppingToken);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                $"Error in [ProductCheckerListener]--> can't save {failCacheKey} in redis or updating OrderId:{orderRequest!.OrderId} in outboxOrder table");

            var _ = await _cache.RemoveData(failCacheKey!, stoppingToken);
        }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
                _logger.LogError("background stopped unexpectedly");

            _hostApplicationLifetime.StopApplication();
        }
    }

    private async Task AddOrderWithStateFailInRedis(
        string redisKey,
        OrderRequest orderRequest,
        CancellationToken stoppingToken)
    {
    }
}