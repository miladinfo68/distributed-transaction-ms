using System.Text.Json;
using CommonService;
using CommonService.Entities;
using CommonService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Data;

namespace OrderService.Jobs;

public class OutBoxOrdersListener : BackgroundService
{
    private readonly ICacheService _cache;
    private readonly IServiceScopeFactory _provider;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private readonly ILogger<OutBoxOrdersListener> _logger;
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly RedisConfigs _redisConfigs;


    public OutBoxOrdersListener(
        ICacheService cache,
        IServiceScopeFactory provider,
        IOptions<RedisConfigs> redisConfigs,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<OutBoxOrdersListener> logger)
    {
        _cache = cache;
        _provider = provider;
        _redisConfigs = redisConfigs.Value;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);
        await using var scope = _provider.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        
        OrderRequest? orderRequest = default;
        string? cacheKey = default;
        
        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                var pendingOrders = db.OutboxOrders
                    .Where(w => w.OrderStatus == OrderStatus.Pending && !w.Tracked)
                    .AsAsyncEnumerable();
                
                await foreach (var outboxOrder in pendingOrders)
                {
                    if (string.IsNullOrEmpty(outboxOrder?.JsonData)) continue;
                    
                    orderRequest = JsonSerializer.Deserialize<OrderRequest>(outboxOrder.JsonData);
                    
                    var pendingCacheKey=_redisConfigs.CacheKeyPendingFormat
                        .GetCacheKeyPattern(orderRequest!.OrderId, orderRequest.OrderDate);

                    if (await _cache.IsExistAnyKey(pendingCacheKey, stoppingToken)) continue;
                    
                    var _ = await _cache.SetData<OrderRequest>(cacheKey, orderRequest, stoppingToken);

                    await db.OutboxOrders
                        .ExecuteUpdateAsync(s => 
                            s.SetProperty(p => p.Tracked, true), cancellationToken: stoppingToken);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Error in [OutBoxOrdersListener]--> can't save {cacheKey} in redis or updating OrderId:{orderRequest!.OrderId} in outboxOrder table");
            
            var _ = await _cache.RemoveData(cacheKey!, stoppingToken);
            
            await db.OutboxOrders
                .ExecuteUpdateAsync(s => 
                    s.SetProperty(p => p.Tracked, false), cancellationToken: stoppingToken);
        }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
                _logger.LogError("background stopped unexpectedly");
            
            _hostApplicationLifetime.StopApplication();
        }
    }
}