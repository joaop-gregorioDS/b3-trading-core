using B3.TradingCore.Application.Interfaces;
using B3.TradingCore.Domain.Interfaces;
using B3.TradingCore.Infrastructure.Cache;
using B3.TradingCore.Infrastructure.Messaging;
using B3.TradingCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace B3.TradingCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Banco de Dados PostgreSQL via Entity Framework Core
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Host=localhost;Port=5432;Database=b3_trading;Username=postgres;Password=postgres";

        services.AddDbContext<TradingDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ITradeRepository, TradeRepository>();

        // 2. Cache Distribuído com Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RedisTradingCacheService>>();
            var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            try
            {
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 2000;
                return ConnectionMultiplexer.Connect(options);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Não foi possível estabelecer conexão inicial com o Redis ({Message}).", ex.Message);
                return null!;
            }
        });

        services.AddSingleton<ITradingCacheService, RedisTradingCacheService>();

        // 3. Mensageria com RabbitMQ
        services.AddSingleton<ITradeMessageProducer, RabbitMqTradeProducer>();

        return services;
    }
}
