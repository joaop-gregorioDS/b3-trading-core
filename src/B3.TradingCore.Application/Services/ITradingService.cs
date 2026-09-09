using B3.TradingCore.Application.DTOs;

namespace B3.TradingCore.Application.Services;

/// <summary>
/// Contrato do serviço orquestrador de operações de trading.
/// </summary>
public interface ITradingService
{
    Task<TradeResponse> ExecuteTradeAsync(CreateTradeRequest request, CancellationToken cancellationToken = default);
    Task<TradeResponse?> GetTradeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TradeResponse>> GetTradesByAccountAsync(string accountId, CancellationToken cancellationToken = default);
    Task<TradeResponse?> GetLatestTickerQuoteAsync(string ticker, CancellationToken cancellationToken = default);
}
