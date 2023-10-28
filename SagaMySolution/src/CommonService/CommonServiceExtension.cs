using CommonService.Entities;
using CommonService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CommonService;

public static class CommonServiceExtension
{
    public static IServiceCollection RegisterCommonServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        //var redisConfigs = configuration.GetSection("RedisConfigs").Get<RedisConfigs>();
        services.Configure<RedisConfigs>(options => configuration.GetSection("RedisConfigs").Bind(options));
        
        //if (redisConfigs is null) return services;
     
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var redisConfigs = provider.GetRequiredService<IOptions<RedisConfigs>>().Value;
           return ConnectionMultiplexer.Connect(redisConfigs.Server);
        });

        services.AddSingleton<ICacheService>(provider =>
        {
            var redisConnectionMultiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            var redisConfigs = provider.GetRequiredService<IOptions<RedisConfigs>>().Value;
            return new CacheService(redisConnectionMultiplexer, DateTimeOffset.UtcNow.AddHours(redisConfigs.CacheExpireTimeHour));
        });

        return services;
    }
}

public static class NormalizeKeyExtension
{
    public static string GetCacheKeyPattern(this string cacheKey ,int orderId ,DateTime orderDate)
    {
        return string.Join("-", string.Format(cacheKey ,orderId, orderDate).Split('-')[0..3]) + "-*";
    }
}