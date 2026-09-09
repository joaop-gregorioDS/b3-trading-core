using B3.TradingCore.Application.DTOs;

namespace B3.TradingCore.Application.Interfaces;

/// <summary>
/// Contrato para operações de cache distribuído em memória (Redis).
/// Alta performance para cotações e resumo de custódia.
/// </summary>
public interface ITradingCacheService
{
    Task CacheLatestTradeAsync(TradeResponse trade, CancellationToken cancellationToken = default);
    Task<TradeResponse?> GetLatestTradeAsync(string ticker, CancellationToken cancellationToken = default);
    Task IncrementDailyVolumeAsync(string ticker, decimal amount, CancellationToken cancellationToken = default);
}
