using CommonService.Entities;
using CommonService.Services;
using PaymentService.Services;

namespace PaymentService.Jobs;

public class PaymentCheckerListener : BackgroundService
{
    private readonly ICacheService _cache;
    private readonly IPaymentService _paymentService;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
    private readonly ILogger<PaymentCheckerListener> _logger;

    public PaymentCheckerListener(
        ICacheService cache,
        IPaymentService paymentService,
        ILogger<PaymentCheckerListener> logger)
    {
        _cache = cache;
        _logger = logger;
        _paymentService = paymentService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_period);
        const string pattern = "order-*";
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var redisPendingOrders = _cache.GetKeysAndValuesByPattern2<OrderRequest>(pattern, stoppingToken);
            await foreach (var (redisKey, orderRequest) in redisPendingOrders)
            {
                if (string.IsNullOrEmpty(redisKey)
                    || orderRequest.OrderStatus != OrderStatus.Pending
                    || !orderRequest.OrderItems.Any()
                   ) continue;
                //update request state in redis to fail and set some messages
            }
        }
    }

   
}