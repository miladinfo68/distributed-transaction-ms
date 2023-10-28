using CommonService.Entities;
using CommonService.Services;
using UserService.Data;

namespace UserService.Jobs;

public class UserNotificationListener : BackgroundService
{
    private readonly ICacheService _cache;
    private readonly IServiceScopeFactory _provider;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);
    private readonly ILogger<UserNotificationListener> _logger;

    public UserNotificationListener(
        ICacheService cache,
        ILogger<UserNotificationListener> logger,
        IServiceScopeFactory provider)
    {
        _cache = cache;
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);
        const string pattern = "order-*";

        await using var scope = _provider.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var redisNewOrders = _cache.GetKeysAndValuesByPattern2<OrderRequest>(pattern, stoppingToken);
            await foreach (var (redisKey, orderRequest) in redisNewOrders)
            {
                if(string.IsNullOrEmpty(redisKey)) continue;
                
            }
            
            
            
            // var pendingNewOrders = await _cache.GetKeysAndValuesByPattern<OrderRequest>(pattern, stoppingToken);
            // var i = 2;
            // await Parallel.ForEachAsync(pendingNewOrders, stoppingToken, async (kvOrder, _) =>
            // {
            //     var (redisKey, orderRequest) = kvOrder;
            //     await _cache.UpdateField<OrderRequest, int>("order-1-20231025193140", "Version", i++, stoppingToken);
            // });
            // await Task.Delay(100, stoppingToken);
            
        }
    }
}