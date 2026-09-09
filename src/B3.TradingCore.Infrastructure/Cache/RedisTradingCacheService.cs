using System.Text.Json;
using B3.TradingCore.Application.DTOs;
using B3.TradingCore.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace B3.TradingCore.Infrastructure.Cache;

/// <summary>
/// Serviço de cache distribuído usando Redis (StackExchange.Redis).
/// Utilizado para manter cotações recentes da B3 e acumular volume financeiro diário.
/// </summary>
public class RedisTradingCacheService : ITradingCacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisTradingCacheService> _logger;
    private readonly TimeSpan _quoteTtl = TimeSpan.FromHours(24);

    public RedisTradingCacheService(
        ILogger<RedisTradingCacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task CacheLatestTradeAsync(TradeResponse trade, CancellationToken cancellationToken = default)
    {
        if (_redis is null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis não está conectado. Operação de cache ignorada.");
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var key = $"b3:quotes:{trade.Ticker.ToUpperInvariant()}";
            var json = JsonSerializer.Serialize(trade);

            await db.StringSetAsync(key, json, _quoteTtl);
            _logger.LogInformation("Cotação recente de {Ticker} atualizada no Redis.", trade.Ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gravar cotação no Redis para o papel {Ticker}.", trade.Ticker);
        }
    }

    public async Task<TradeResponse?> GetLatestTradeAsync(string ticker, CancellationToken cancellationToken = default)
    {
        if (_redis is null || !_redis.IsConnected)
        {
            return null;
        }

        try
        {
            var db = _redis.GetDatabase();
            var key = $"b3:quotes:{ticker.ToUpperInvariant()}";
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<TradeResponse>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar cotação no Redis para o papel {Ticker}.", ticker);
            return null;
        }
    }

    public async Task IncrementDailyVolumeAsync(string ticker, decimal amount, CancellationToken cancellationToken = default)
    {
        if (_redis is null || !_redis.IsConnected)
        {
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var key = $"b3:volume:{ticker.ToUpperInvariant()}:{today}";

            // Incrementa o volume financeiro acumulado no dia
            await db.StringIncrementAsync(key, (double)amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao incrementar volume financeiro diário no Redis para {Ticker}.", ticker);
        }
    }
}
